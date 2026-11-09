using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniQuest.Contracts
{
    /// <summary>
    /// Ce que l'UI a besoin de savoir sur un personnage (joueur ou ennemi).
    /// La classe Character d'Ilies implémente cette interface.
    /// L'UI ne fait que LIRE ces valeurs et ÉCOUTER ces événements : elle ne modifie jamais un personnage.
    /// </summary>
    public interface ICombatant
    {
        string Name { get; }
        string TypeName { get; }          // "Fer", "Chair", "Lumière", "Alchimie"...
        int Level { get; }

        int CurrentHP { get; }
        int MaxHP { get; }
        int CurrentMP { get; }
        int MaxMP { get; }
        int CurrentXP { get; }
        int XPToNextLevel { get; }

        int Attack { get; }               // attaque TOTALE (bonus d'arme compris)
        int Defense { get; }
        int Speed { get; }
        int Accuracy { get; }             // en pourcentage, 0-100

        bool IsKO { get; }

        Sprite BattleSprite { get; }      // sprite affiché en combat
        Sprite Portrait { get; }          // sprite affiché dans les menus (peut être le même)

        /// <summary>Toutes les attaques, débloquées ou non (UnlockLevel indique quand).</summary>
        IReadOnlyList<IAttackInfo> Attacks { get; }

        /// <summary>Arme équipée, ou null (les ennemis n'en ont pas forcément).</summary>
        IWeaponInfo Weapon { get; }

        event Action<int, int> OnHealthChanged;      // (actuel, max)
        event Action<int, int> OnManaChanged;        // (actuel, max)
        event Action<int, int> OnExperienceChanged;  // (actuel, requis pour le niveau suivant)
        event Action<int> OnLevelChanged;            // (nouveau niveau)
    }

    public interface IAttackInfo
    {
        string Name { get; }
        string TypeName { get; }
        int Power { get; }
        int Accuracy { get; }             // pourcentage
        int CritChance { get; }           // pourcentage
        int ManaCost { get; }             // 0 pour une attaque physique
        bool IsMagic { get; }
        int UnlockLevel { get; }          // niveau requis pour l'utiliser
        string Description { get; }
    }

    public interface IWeaponInfo
    {
        string Name { get; }
        int UpgradeLevel { get; }         // +0, +1, +2...
        int AttackBonus { get; }
        Sprite Icon { get; }
    }
}
