using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniQuest.Contracts;

namespace UniQuest.UI.DebugTools
{
    /// <summary>
    /// FAUX GAMEPLAY pour tester toute l'UI sans attendre le code d'Ilies :
    /// un vrai petit combat (Trochu, Docteur Bec, Gaspard contre une larve ou l'amas),
    /// un inventaire, une équipe et 3 emplacements de sauvegarde en mémoire.
    ///
    /// Utilisation : pose ce composant sur un GameObject vide de ta scène de test,
    /// glisse les sprites dans l'Inspector, lance le jeu. L'UI le trouve toute seule.
    /// Il sert aussi d'EXEMPLE pour Ilies : c'est exactement ce que son code doit exposer.
    ///
    /// Le hasard passe par System.Random(seed) : même seed = même combat. C'est le principe
    /// du système de seed demandé par le sujet.
    /// </summary>
    public class FakeGameplay : MonoBehaviour, IFightController, IInventory, IParty, ISaveService
    {
        [Header("Sprites (glisse les PNG de l'équipe)")]
        [SerializeField] private Sprite trochuSprite;
        [SerializeField] private Sprite docteurBecSprite;
        [SerializeField] private Sprite gaspardSprite;
        [SerializeField] private Sprite larveSprite;
        [SerializeField] private Sprite amasSprite;
        [SerializeField] private Sprite potionRedIcon;
        [SerializeField] private Sprite potionBlueIcon;
        [SerializeField] private Sprite eyeIcon;
        [SerializeField] private Sprite tailIcon;

        [Header("Test")]
        [SerializeField] private int seed = 12345;
        [Tooltip("Lance un combat dès le Play (scène de combat). Décoche pour tester la carte et les menus.")]
        [SerializeField] private bool startFightOnPlay = true;
        [Tooltip("Combat contre l'amas (ennemi évolué) au lieu de la larve")]
        [SerializeField] private bool eliteFight = false;

        private System.Random rng;
        private readonly List<FakeCombatant> team = new List<FakeCombatant>();
        private readonly List<FakeItem> items = new List<FakeItem>();
        private FakeCombatant active;
        private FakeCombatant enemy;

        // Communication avec l'UI
        private bool uiReady;
        private Action pendingChoice;      // action choisie par le joueur, exécutée par la boucle de combat
        private bool waitingForSwitch;

        // ───────────────────────── Interfaces ─────────────────────────

        public ICombatant ActivePlayer => active;
        public ICombatant Enemy => enemy;
        public IReadOnlyList<ICombatant> Team => team;
        public IReadOnlyList<ICombatant> Members => team;
        public bool IsEliteEnemy => eliteFight;
        public bool CanFlee => !eliteFight;
        public IReadOnlyList<IItemInfo> Items => items;

        public event Action OnFightStarted;
        public event Action<ICombatant> OnPlayerTurn;
        public event Action<ActionResult> OnActionPerformed;
        public event Action<ICombatant> OnSwitchRequired;
        public event Action<ICombatant> OnActivePlayerChanged;
#pragma warning disable 0067   // pas d'évolution dans le faux gameplay
        public event Action<ICombatant> OnEnemyChanged;
#pragma warning restore 0067
        public event Action<string> OnInfoMessage;
        public event Action<FightOutcome> OnFightEnded;
        public event Action OnInventoryChanged;
        public event Action OnPartyChanged;

        // ───────────────────────── Mise en place ─────────────────────────

        private void Awake()
        {
            rng = new System.Random(seed);
            BuildTeam();
            BuildInventory();
        }

        private void Start()
        {
            OnPartyChanged?.Invoke();
            if (startFightOnPlay) StartCoroutine(FightLoop());
        }

        private void BuildTeam()
        {
            var trochu = new FakeCombatant(6, 90, 20)
            { Name = "Trochu", TypeName = "Fer", BaseAttack = 20, Defense = 18, Speed = 12, Accuracy = 90, BattleSprite = trochuSprite };
            trochu.FakeWeapon = new FakeWeapon { Name = "Casque de Trochu" };
            trochu.AttackList.Add(new FakeAttack { Name = "Coup de casque", TypeName = "Fer", Power = 40, Description = "Il tape avec son casque. Personne ne sait pourquoi ça marche." });
            trochu.AttackList.Add(new FakeAttack { Name = "Charge maladroite", TypeName = "Fer", Power = 70, Accuracy = 60, CritChance = 25, UnlockLevel = 5, Description = "Puissante, si elle touche." });
            trochu.AttackList.Add(new FakeAttack { Name = "Cri de panique", TypeName = "Fer", Power = 55, ManaCost = 8, UnlockLevel = 8, Description = "Débloquée au niveau 8." });

            var bec = new FakeCombatant(5, 60, 40)
            { Name = "Docteur Bec", TypeName = "Alchimie", BaseAttack = 14, Defense = 12, Speed = 14, Accuracy = 98, BattleSprite = docteurBecSprite };
            bec.AttackList.Add(new FakeAttack { Name = "Coup de scalpel", TypeName = "Alchimie", Power = 30 });
            bec.AttackList.Add(new FakeAttack { Name = "Nuage de rouille", TypeName = "Alchimie", Power = 45, ManaCost = 6, Description = "Fait rouiller le métal." });

            var gaspard = new FakeCombatant(5, 48, 50)
            { Name = "Gaspard", TypeName = "Lumière", BaseAttack = 12, Defense = 8, Speed = 10, Accuracy = 85, BattleSprite = gaspardSprite };
            gaspard.AttackList.Add(new FakeAttack { Name = "Coup de bâton", TypeName = "Lumière", Power = 25 });
            gaspard.AttackList.Add(new FakeAttack { Name = "Éclat de lanterne", TypeName = "Lumière", Power = 50, ManaCost = 8, Description = "La lumière brûle les chairs." });
            gaspard.AttackList.Add(new FakeAttack { Name = "Aube", TypeName = "Lumière", Power = 90, ManaCost = 60, Description = "Coûte plus de PM que Gaspard n'en a : grisé." });

            team.Add(trochu); team.Add(bec); team.Add(gaspard);
            active = trochu;

            enemy = eliteFight
                ? new FakeCombatant(12, 160, 0) { Name = "Amas évolué", TypeName = "Chair", BaseAttack = 26, Defense = 14, Speed = 16, BattleSprite = amasSprite }
                : new FakeCombatant(4, 60, 0) { Name = "Larve mutante", TypeName = "Chair", BaseAttack = 16, Defense = 8, Speed = 9, BattleSprite = larveSprite };
            enemy.AttackList.Add(new FakeAttack { Name = eliteFight ? "Taillade" : "Morsure", TypeName = "Chair", Power = eliteFight ? 45 : 30, Accuracy = 90 });
        }

        private void BuildInventory()
        {
            items.Add(new FakeItem { Name = "Potion de vie", Description = "Rend 30 PV.", Icon = potionRedIcon, Category = ItemCategory.Heal, Quantity = 3, HealHP = 30 });
            items.Add(new FakeItem { Name = "Potion de mana", Description = "Rend 15 PM.", Icon = potionBlueIcon, Category = ItemCategory.Heal, Quantity = 1, HealMP = 15 });
            items.Add(new FakeItem { Name = "Pain rassis", Description = "Rend 10 PV. Craque sous la dent.", Category = ItemCategory.Snack, Quantity = 2, HealHP = 10 });
            items.Add(new FakeItem { Name = "Œil de larve", Description = "Lâché par les larves. Sert à améliorer l'arme.", Icon = eyeIcon, Category = ItemCategory.Material, Quantity = 4 });
            items.Add(new FakeItem { Name = "Queue de larve", Description = "Lâchée par les larves. Sert à améliorer l'arme.", Icon = tailIcon, Category = ItemCategory.Material, Quantity = 2 });
            items.Add(new FakeItem { Name = "Clé rouillée", Description = "Ouvre quelque chose, quelque part.", Category = ItemCategory.Key, Quantity = 1 });
        }

        // ───────────────────────── Boucle de combat ─────────────────────────

        public void NotifyUIReady() => uiReady = true;

        /// <summary>Déclenche un événement puis attend que l'UI ait fini de l'afficher.</summary>
        private IEnumerator RaiseAndWait(Action raise)
        {
            uiReady = false;   // AVANT l'événement
            raise();
            while (!uiReady) yield return null;
        }

        private IEnumerator FightLoop()
        {
            yield return null;   // laisse l'UI s'initialiser
            yield return RaiseAndWait(() => OnFightStarted?.Invoke());

            while (true)
            {
                bool playerFirst = active.Speed >= enemy.Speed;

                if (playerFirst)
                {
                    yield return PlayerTurn();
                    if (IsOver()) break;
                    yield return EnemyTurn();
                }
                else
                {
                    yield return EnemyTurn();
                    if (IsOver()) break;
                    yield return PlayerTurn();
                }
                if (IsOver()) break;

                // Régénération progressive du mana (demandée par le sujet)
                foreach (var m in team) if (!m.IsKO) m.ChangeMP(2);
            }
        }

        private bool fightOver;
        private bool IsOver() => fightOver;

        private IEnumerator PlayerTurn()
        {
            pendingChoice = null;
            OnPlayerTurn?.Invoke(active);
            while (pendingChoice == null) yield return null;

            var choice = pendingChoice;
            pendingChoice = null;
            uiReady = false;
            choice();                       // déclenche OnActionPerformed (ou fuite / changement)
            while (!uiReady && !fightOver) yield return null;

            if (!fightOver && enemy.IsKO) yield return Victory();
        }

        private IEnumerator EnemyTurn()
        {
            if (enemy.IsKO || fightOver) yield break;

            var attack = enemy.Attacks[rng.Next(enemy.Attacks.Count)];
            var result = Resolve(enemy, active, attack);
            yield return RaiseAndWait(() => OnActionPerformed?.Invoke(result));

            if (!active.IsKO) yield break;

            bool anyAlive = team.Exists(m => !m.IsKO);
            if (!anyAlive)
            {
                fightOver = true;
                yield return RaiseAndWait(() => OnFightEnded?.Invoke(new FightOutcome { PlayerWon = false }));
                yield break;
            }

            waitingForSwitch = true;
            OnSwitchRequired?.Invoke(active);
            while (waitingForSwitch) yield return null;
            while (!uiReady) yield return null;
        }

        private IEnumerator Victory()
        {
            fightOver = true;
            int xp = eliteFight ? 60 : 25;
            var eye = items.Find(i => i.Name == "Œil de larve");
            if (eye == null)
            {
                eye = new FakeItem { Name = "Œil de larve", Description = "Lâché par les larves. Sert à améliorer l'arme.", Icon = eyeIcon, Category = ItemCategory.Material };
                items.Add(eye);
            }
            var drops = new List<IItemInfo> { eye };

            yield return RaiseAndWait(() => OnFightEnded?.Invoke(new FightOutcome
            { PlayerWon = true, ExperienceGained = xp, Drops = drops }));

            foreach (var m in team)
                if (!m.IsKO && m.GainXP(xp))
                    yield return RaiseAndWait(() => OnInfoMessage?.Invoke($"{m.Name} passe au niveau {m.Level} !"));

            eye.Quantity++;
            OnInventoryChanged?.Invoke();
        }

        private ActionResult Resolve(FakeCombatant attacker, FakeCombatant target, IAttackInfo attack)
        {
            if (attack.ManaCost > 0) attacker.ChangeMP(-attack.ManaCost);

            var r = new ActionResult { Actor = attacker, Target = target, ActionName = attack.Name };
            int hitChance = attack.Accuracy * attacker.Accuracy / 100;
            if (rng.Next(100) >= hitChance) { r.IsMiss = true; return r; }

            float dmg = attack.Power * attacker.Attack / (float)Mathf.Max(1, target.Defense) * 0.35f;
            dmg *= 0.85f + (float)rng.NextDouble() * 0.15f;

            r.Effectiveness = Effect(attack.TypeName, target.TypeName);
            if (r.Effectiveness == Effectiveness.SuperEffective) dmg *= 1.5f;
            if (r.Effectiveness == Effectiveness.NotVeryEffective) dmg *= 0.6f;
            if (rng.Next(100) < attack.CritChance) { r.IsCritical = true; dmg *= 1.5f; }

            r.Damage = Mathf.Max(1, Mathf.RoundToInt(dmg));
            target.ChangeHP(-r.Damage);
            return r;
        }

        // Cycle : Lumière > Chair > Alchimie > Fer > Lumière
        private static Effectiveness Effect(string attackType, string targetType)
        {
            string[] cycle = { "Lumière", "Chair", "Alchimie", "Fer" };
            int a = Array.IndexOf(cycle, attackType), t = Array.IndexOf(cycle, targetType);
            if (a < 0 || t < 0) return Effectiveness.Normal;
            if ((a + 1) % 4 == t) return Effectiveness.SuperEffective;
            if ((t + 1) % 4 == a) return Effectiveness.NotVeryEffective;
            return Effectiveness.Normal;
        }

        // ───────────────────────── Choix du joueur (appelés par l'UI) ─────────────────────────

        public void PlayerAttack(IAttackInfo attack)
        {
            pendingChoice = () =>
            {
                var r = Resolve(active, enemy, attack);
                OnActionPerformed?.Invoke(r);
            };
        }

        public void PlayerUseItem(IItemInfo item)
        {
            pendingChoice = () =>
            {
                var fake = (FakeItem)item;
                ApplyItem(fake, active);
                OnActionPerformed?.Invoke(new ActionResult
                { Actor = active, Target = active, ActionName = item.Name, Damage = -fake.HealHP });
            };
        }

        public void PlayerSwitch(ICombatant newActive)
        {
            var target = (FakeCombatant)newActive;
            if (waitingForSwitch)
            {
                // Remplacement après KO : ne consomme pas de tour
                active = target;
                uiReady = false;
                OnActivePlayerChanged?.Invoke(active);
                waitingForSwitch = false;
                return;
            }
            pendingChoice = () =>
            {
                active = target;
                OnActivePlayerChanged?.Invoke(active);
            };
        }

        public void PlayerFlee()
        {
            pendingChoice = () =>
            {
                fightOver = true;
                OnFightEnded?.Invoke(new FightOutcome { PlayerFled = true });
            };
        }

        // ───────────────────────── Inventaire ─────────────────────────

        private void ApplyItem(FakeItem item, FakeCombatant target)
        {
            if (item.HealHP > 0) target.ChangeHP(item.HealHP);
            if (item.HealMP > 0) target.ChangeMP(item.HealMP);
            item.Quantity--;
            if (item.Quantity <= 0) items.Remove(item);
            OnInventoryChanged?.Invoke();
        }

        public bool Use(IItemInfo item, ICombatant target)
        {
            var fake = item as FakeItem;
            var who = target as FakeCombatant;
            if (fake == null || who == null || !item.UsableOutsideFight) return false;
            ApplyItem(fake, who);
            return true;
        }

        public void Discard(IItemInfo item)
        {
            items.Remove(item as FakeItem);
            OnInventoryChanged?.Invoke();
        }

        private int Count(string name)
        {
            var i = items.Find(x => x.Name == name);
            return i != null ? i.Quantity : 0;
        }

        public bool CanUpgradeWeapon(ICombatant owner) =>
            owner.Weapon != null && Count("Œil de larve") >= 2 && Count("Queue de larve") >= 1;

        public string GetUpgradeCost(ICombatant owner) => "2 Œil de larve + 1 Queue de larve";

        public bool UpgradeWeapon(ICombatant owner)
        {
            if (!CanUpgradeWeapon(owner)) return false;
            items.Find(x => x.Name == "Œil de larve").Quantity -= 2;
            items.Find(x => x.Name == "Queue de larve").Quantity -= 1;
            items.RemoveAll(x => x.Quantity <= 0);
            ((FakeCombatant)owner).FakeWeapon.UpgradeLevel++;
            OnInventoryChanged?.Invoke();
            return true;
        }

        // ───────────────────────── Sauvegardes (en mémoire) ─────────────────────────

        private static readonly SaveSlotInfo[] slots =
        {
            new SaveSlotInfo { Index = 0, IsEmpty = false, ZoneName = "Forêt des larves", SavedAt = DateTime.Now.AddHours(-3),
                               PlayTime = TimeSpan.FromMinutes(83), Seed = 12345, PartySummary = "Trochu Niv. 6 · Docteur Bec Niv. 5 · Gaspard Niv. 5" },
            new SaveSlotInfo { Index = 1, IsEmpty = true },
            new SaveSlotInfo { Index = 2, IsEmpty = true },
        };
        private static int lastSlot = 0;

        public int SlotCount => slots.Length;
        public IReadOnlyList<SaveSlotInfo> GetSlots() => slots;
        public bool HasAnySave => Array.Exists(slots, s => !s.IsEmpty);
        public int LastUsedSlot => lastSlot;

        public bool Save(int slotIndex)
        {
            slots[slotIndex] = new SaveSlotInfo
            {
                Index = slotIndex, IsEmpty = false, ZoneName = "Zone de test", SavedAt = DateTime.Now,
                PlayTime = TimeSpan.FromSeconds(Time.realtimeSinceStartup), Seed = seed,
                PartySummary = string.Join(" · ", team.ConvertAll(m => $"{m.Name} Niv. {m.Level}"))
            };
            lastSlot = slotIndex;
            return true;
        }

        public bool Load(int slotIndex)
        {
            if (slots[slotIndex].IsEmpty) return false;
            lastSlot = slotIndex;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return true;
        }
    }
}
