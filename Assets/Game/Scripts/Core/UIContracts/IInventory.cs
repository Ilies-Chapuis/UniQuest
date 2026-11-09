using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniQuest.Contracts
{
    public enum ItemCategory { Heal, Boost, Material, Key, Snack }

    public interface IItemInfo
    {
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }
        ItemCategory Category { get; }
        int Quantity { get; }
        bool UsableInFight { get; }
        bool UsableOutsideFight { get; }
        bool NeedsTarget { get; }        // une potion se donne à un perso, une clé non
    }

    /// <summary>Inventaire du joueur, implémenté côté gameplay.</summary>
    public interface IInventory
    {
        IReadOnlyList<IItemInfo> Items { get; }

        /// <summary>Déclenché à chaque ajout, utilisation ou suppression.</summary>
        event Action OnInventoryChanged;

        /// <summary>Utilise un objet hors combat. target peut être null si NeedsTarget est false.</summary>
        bool Use(IItemInfo item, ICombatant target);

        void Discard(IItemInfo item);

        /// <summary>Le perso a-t-il assez de matériaux pour améliorer son arme ?</summary>
        bool CanUpgradeWeapon(ICombatant owner);

        /// <summary>Texte du coût, ex. "2 Œil de larve + 1 Queue de larve".</summary>
        string GetUpgradeCost(ICombatant owner);

        bool UpgradeWeapon(ICombatant owner);
    }

    /// <summary>Accès à l'équipe hors combat (menu pause).</summary>
    public interface IParty
    {
        IReadOnlyList<ICombatant> Members { get; }
        event Action OnPartyChanged;
    }
}
