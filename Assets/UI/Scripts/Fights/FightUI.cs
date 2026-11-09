using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Chef d'orchestre de l'écran de combat. Il écoute le FightManager et met en scène :
    ///  - les messages s'affichent UN PAR UN, dans l'ordre (file d'attente) ;
    ///  - les barres de PV bougent au moment où l'attaque touche à l'écran ;
    ///  - les menus s'ouvrent quand c'est au joueur de jouer ;
    ///  - quand tout est affiché, il appelle fight.NotifyUIReady() pour que le combat continue.
    ///
    /// Il ne calcule RIEN : dégâts, IA, XP, ordre des tours restent dans le code d'Ilies.
    /// </summary>
    public class FightUI : MonoBehaviour
    {
        [Header("Gameplay (vide = recherche automatique dans la scène)")]
        [SerializeField] private MonoBehaviour fightSource;
        [SerializeField] private MonoBehaviour inventorySource;

        [Header("Combattants")]
        [SerializeField] private CharacterHUD playerHUD;
        [SerializeField] private CharacterHUD enemyHUD;
        [SerializeField] private BattlerView playerView;
        [SerializeField] private BattlerView enemyView;

        [Header("Menus et textes")]
        [SerializeField] private FightLog log;
        [SerializeField] private ActionMenu actionMenu;
        [SerializeField] private AttackMenu attackMenu;
        [SerializeField] private FightItemMenu itemMenu;
        [SerializeField] private PartySelectPanel partySelect;
        [SerializeField] private GameOverScreen gameOverScreen;

        [Header("Ambiance (optionnel)")]
        [Tooltip("Image de fond teintée en rouge contre un ennemi évolué ou un boss")]
        [SerializeField] private Image backgroundTint;
        [SerializeField] private Color normalTint = Color.white;
        [SerializeField] private Color eliteTint = new Color32(0xFF, 0xB0, 0xB0, 0xFF);

        /// <summary>
        /// Déclenché quand tous les messages de fin sont affichés (victoire ou fuite).
        /// C'est le moment de revenir sur la carte : le gameplay peut s'y abonner.
        /// En cas de défaite, c'est l'écran de Game Over qui prend le relais.
        /// </summary>
        public event Action<FightOutcome> OnEndSequenceFinished;

        private IFightController fight;
        private IInventory inventory;
        private readonly Queue<Func<IEnumerator>> steps = new Queue<Func<IEnumerator>>();
        private Coroutine runner;

        private void Awake()
        {
            fight = UIServices.Resolve<IFightController>(fightSource);
            inventory = UIServices.Resolve<IInventory>(inventorySource);
            if (fight == null)
            {
                Debug.LogError("[FightUI] Aucun IFightController trouvé dans la scène. " +
                               "Ajoute le FightManager (ou FakeFightController pour tester).", this);
                enabled = false;
                return;
            }

            fight.OnFightStarted += HandleFightStarted;
            fight.OnPlayerTurn += HandlePlayerTurn;
            fight.OnActionPerformed += HandleActionPerformed;
            fight.OnSwitchRequired += HandleSwitchRequired;
            fight.OnActivePlayerChanged += HandleActivePlayerChanged;
            fight.OnEnemyChanged += HandleEnemyChanged;
            fight.OnInfoMessage += HandleInfoMessage;
            fight.OnFightEnded += HandleFightEnded;

            actionMenu.OnAttack += ChooseAttack;
            actionMenu.OnMagic += ChooseMagic;
            actionMenu.OnItem += ChooseItem;
            actionMenu.OnTeam += ChooseTeam;
            actionMenu.OnFlee += ChooseFlee;
            attackMenu.OnAttackChosen += SubmitAttack;
            itemMenu.OnItemChosen += SubmitItem;

            playerHUD.SetDeferred(true);
            enemyHUD.SetDeferred(true);
        }

        private void OnDestroy()
        {
            if (fight == null) return;
            fight.OnFightStarted -= HandleFightStarted;
            fight.OnPlayerTurn -= HandlePlayerTurn;
            fight.OnActionPerformed -= HandleActionPerformed;
            fight.OnSwitchRequired -= HandleSwitchRequired;
            fight.OnActivePlayerChanged -= HandleActivePlayerChanged;
            fight.OnEnemyChanged -= HandleEnemyChanged;
            fight.OnInfoMessage -= HandleInfoMessage;
            fight.OnFightEnded -= HandleFightEnded;
        }

        // ───────────────────────── File d'attente ─────────────────────────

        private void Enqueue(Func<IEnumerator> step)
        {
            steps.Enqueue(step);
            if (runner == null) runner = StartCoroutine(RunSteps());
        }

        private IEnumerator RunSteps()
        {
            // On attend toujours une frame : NotifyUIReady ne doit jamais être appelé
            // pendant que le gameplay est encore en train de déclencher son événement.
            yield return null;

            while (steps.Count > 0)
                yield return steps.Dequeue()();

            runner = null;
            // Tout est affiché : le gameplay peut passer à la suite.
            // (Si NotifyUIReady déclenche de nouveaux événements, ils relancent une file.)
            fight.NotifyUIReady();
        }

        // ───────────────────────── Événements du gameplay ─────────────────────────

        private void HandleFightStarted()
        {
            bool elite = fight.IsEliteEnemy;
            playerHUD.Bind(fight.ActivePlayer);
            enemyHUD.Bind(fight.Enemy);
            enemyHUD.SetElite(elite);
            playerView.Bind(fight.ActivePlayer);
            enemyView.Bind(fight.Enemy, elite);
            if (backgroundTint != null) backgroundTint.color = elite ? eliteTint : normalTint;

            Enqueue(() => log.Say(FightMessages.Intro(fight.Enemy, elite)));
        }

        private void HandlePlayerTurn(ICombatant player) => Enqueue(() => OpenActionMenu(player));

        private void HandleActionPerformed(ActionResult result) => Enqueue(() => PlayAction(result));

        private void HandleSwitchRequired(ICombatant koMember) => Enqueue(() => AskReplacement(koMember));

        private void HandleActivePlayerChanged(ICombatant newActive) => Enqueue(() => SwitchIn(newActive));

        private void HandleEnemyChanged(ICombatant enemy) => Enqueue(() => EnemyChanges(enemy));

        private void HandleInfoMessage(string message) => Enqueue(() => log.Say(message));

        private void HandleFightEnded(FightOutcome outcome) => Enqueue(() => EndSequence(outcome));

        // ───────────────────────── Étapes affichées ─────────────────────────

        private IEnumerator OpenActionMenu(ICombatant player)
        {
            FlushHUDs();
            log.Prompt(FightMessages.Prompt(player));

            bool canSwitch = false;
            foreach (var m in fight.Team)
                if (!m.IsKO && m != fight.ActivePlayer) { canSwitch = true; break; }

            actionMenu.Show(
                canUseMagic: AttackMenu.Available(player, true).Count > 0,
                hasItems: FightItemMenu.Usable(inventory).Count > 0,
                canSwitch: canSwitch,
                canFlee: fight.CanFlee);
            yield break;   // le menu reste ouvert, la file continue (le gameplay attend le choix)
        }

        private IEnumerator PlayAction(ActionResult r)
        {
            var actorView = ViewFor(r.Actor);
            var targetView = ViewFor(r.Target);
            var messages = FightMessages.Describe(r);

            // 1. "X utilise Y !" pendant que l'attaquant prend son élan
            if (actorView != null) StartCoroutine(actorView.Lunge());
            FlushFor(r.Actor);                                // les PM dépensés
            yield return log.Say(messages[0]);

            // 2. L'impact : animation + barre de PV qui descend
            if (targetView != null)
            {
                if (r.IsMiss) yield return targetView.Miss();
                else if (r.Damage > 0) yield return targetView.Hit();
            }
            FlushFor(r.Target);

            // 3. Les messages suivants (critique, efficacité, dégâts, KO)
            for (int i = 1; i < messages.Count; i++)
            {
                bool isKOMessage = r.Target != null && r.Target.IsKO && i == messages.Count - 1;
                if (isKOMessage && targetView != null) yield return targetView.Faint();
                yield return log.Say(messages[i]);
            }
        }

        private IEnumerator AskReplacement(ICombatant koMember)
        {
            FlushHUDs();
            bool done = false;
            partySelect.Show(
                title: $"{koMember.Name} est KO ! Qui prend le relais ?",
                members: fight.Team,
                canPick: m => !m.IsKO,
                picked: m => { done = true; fight.PlayerSwitch(m); },
                allowCancel: false,
                hint: "Si toute l'équipe est KO, c'est Game Over.");
            while (!done) yield return null;
        }

        private IEnumerator SwitchIn(ICombatant newActive)
        {
            playerHUD.Bind(newActive);
            playerView.Bind(newActive);
            yield return log.Say(FightMessages.SwitchIn(newActive));
        }

        /// <summary>L'ennemi a évolué : nouveau nom, nouveau sprite, ambiance d'ennemi évolué.</summary>
        private IEnumerator EnemyChanges(ICombatant enemy)
        {
            bool elite = fight.IsEliteEnemy;
            yield return enemyView.Hit();          // il se tord...
            enemyHUD.Bind(enemy);
            enemyHUD.SetElite(elite);
            enemyView.Bind(enemy, elite);
            if (backgroundTint != null) backgroundTint.color = elite ? eliteTint : normalTint;
        }

        private IEnumerator EndSequence(FightOutcome outcome)
        {
            CloseMenus();
            FlushHUDs();

            if (outcome.PlayerFled)
            {
                yield return log.Say(FightMessages.Fled);
            }
            else if (outcome.PlayerWon)
            {
                yield return log.Say(FightMessages.Victory);
                if (outcome.ExperienceGained > 0) yield return log.Say(FightMessages.Experience(outcome.ExperienceGained));
                FlushHUDs();   // la barre d'XP monte
                if (outcome.Drops != null)
                    foreach (var drop in outcome.Drops) yield return log.Say(FightMessages.Drop(drop));
            }
            else
            {
                yield return log.Say(FightMessages.Defeat);
                if (gameOverScreen != null) gameOverScreen.Open();
                yield break;
            }

            OnEndSequenceFinished?.Invoke(outcome);
        }

        // ───────────────────────── Choix du joueur ─────────────────────────

        private void ChooseAttack()
        {
            var attacks = AttackMenu.Available(fight.ActivePlayer, false);
            if (attacks.Count == 1) SubmitAttack(attacks[0]);   // une seule attaque : pas de sous-menu
            else attackMenu.Show(fight.ActivePlayer, false);
        }

        private void ChooseMagic() => attackMenu.Show(fight.ActivePlayer, true);

        private void ChooseItem() => itemMenu.Show(inventory);

        private void ChooseTeam()
        {
            var active = fight.ActivePlayer;
            partySelect.Show(
                title: "Qui envoyer au combat ?",
                members: fight.Team,
                canPick: m => !m.IsKO && m != active,
                picked: m => { CloseMenus(); fight.PlayerSwitch(m); },
                allowCancel: true,
                active: active);
        }

        private void ChooseFlee()
        {
            CloseMenus();
            fight.PlayerFlee();
        }

        private void SubmitAttack(IAttackInfo attack)
        {
            CloseMenus();
            fight.PlayerAttack(attack);
        }

        private void SubmitItem(IItemInfo item)
        {
            CloseMenus();
            fight.PlayerUseItem(item);
        }

        // ───────────────────────── Utilitaires ─────────────────────────

        private void CloseMenus()
        {
            attackMenu.Close();
            itemMenu.Close();
            partySelect.Close();
            actionMenu.Close();
        }

        private void FlushHUDs()
        {
            playerHUD.Flush();
            enemyHUD.Flush();
        }

        private BattlerView ViewFor(ICombatant c)
        {
            if (c == null) return null;
            if (c == fight.Enemy) return enemyView;
            if (c == playerHUD.Character) return playerView;
            return null;
        }

        private void FlushFor(ICombatant c)
        {
            if (c == null) return;
            if (c == enemyHUD.Character) enemyHUD.Flush();
            else if (c == playerHUD.Character) playerHUD.Flush();
        }
    }
}
