using System;
using System.Collections.Generic;

namespace UniQuest.Contracts
{
    /// <summary>
    /// Ce que le FightManager d'Ilies expose à l'UI.
    /// - Les ÉVÉNEMENTS disent à l'UI ce qui se passe (elle affiche).
    /// - Les MÉTHODES Player... sont appelées par l'UI quand le joueur choisit (elle relaie).
    /// Le calcul des dégâts, l'IA, l'XP : tout ça reste côté gameplay.
    /// </summary>
    public interface IFightController
    {
        ICombatant ActivePlayer { get; }              // le perso du joueur actuellement au combat
        ICombatant Enemy { get; }
        IReadOnlyList<ICombatant> Team { get; }        // toute l'équipe du joueur
        bool IsEliteEnemy { get; }                     // ennemi évolué ou boss : l'UI le met en avant
        bool CanFlee { get; }                          // false contre un boss

        /// <summary>Le combat commence : l'UI se branche sur les deux combattants.</summary>
        event Action OnFightStarted;

        /// <summary>C'est au joueur de choisir une action : l'UI ouvre le menu d'action.</summary>
        event Action<ICombatant> OnPlayerTurn;

        /// <summary>Une action a été jouée (par le joueur ou l'ennemi) : l'UI affiche le message.</summary>
        event Action<ActionResult> OnActionPerformed;

        /// <summary>Le perso actif est KO et il reste des remplaçants : l'UI ouvre l'écran de remplacement.</summary>
        event Action<ICombatant> OnSwitchRequired;

        /// <summary>Le perso actif a changé (remplacement volontaire ou après KO).</summary>
        event Action<ICombatant> OnActivePlayerChanged;

        /// <summary>L'ennemi a changé (la larve a évolué en amas) : l'UI met à jour son HUD et son sprite.</summary>
        event Action<ICombatant> OnEnemyChanged;

        /// <summary>Un perso a gagné un niveau ou débloqué une attaque : message à afficher.</summary>
        event Action<string> OnInfoMessage;

        /// <summary>Fin du combat.</summary>
        event Action<FightOutcome> OnFightEnded;

        void PlayerAttack(IAttackInfo attack);
        void PlayerUseItem(IItemInfo item);
        void PlayerSwitch(ICombatant newActive);
        void PlayerFlee();

        /// <summary>
        /// L'UI appelle ceci quand elle a fini d'afficher les messages d'une action.
        /// Le gameplay attend ce signal avant d'enchaîner (sinon les messages défilent trop vite).
        /// </summary>
        void NotifyUIReady();
    }

    public struct ActionResult
    {
        public ICombatant Actor;
        public ICombatant Target;
        public string ActionName;       // nom de l'attaque ou de l'objet
        public int Damage;              // négatif = soin
        public bool IsMiss;
        public bool IsCritical;
        public Effectiveness Effectiveness;
        public string CustomMessage;    // optionnel : remplace le message généré par l'UI
    }

    public enum Effectiveness { Normal, SuperEffective, NotVeryEffective }

    public struct FightOutcome
    {
        public bool PlayerWon;
        public bool PlayerFled;
        public int ExperienceGained;
        public IReadOnlyList<IItemInfo> Drops;   // œil, queue... peut être null
    }
}
