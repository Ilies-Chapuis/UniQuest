using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Le menu "Objet" en combat : liste des objets utilisables pendant un combat
    /// (potions, boosts, snacks), avec la description de l'objet sélectionné.
    /// L'objet s'applique au perso actuellement au combat.
    /// </summary>
    public class FightItemMenu : MenuPanel
    {
        [SerializeField] private Transform listContainer;
        [SerializeField] private ItemRowButton itemPrefab;
        [SerializeField] private TMP_Text descriptionText;

        public event Action<IItemInfo> OnItemChosen;

        public static List<IItemInfo> Usable(IInventory inventory)
        {
            var list = new List<IItemInfo>();
            if (inventory == null) return list;
            foreach (var item in inventory.Items)
                if (item.UsableInFight && item.Quantity > 0) list.Add(item);
            return list;
        }

        public void Show(IInventory inventory)
        {
            UINavigation.ClearChildren(listContainer);
            var selectables = new List<Selectable>();
            IItemInfo firstItem = null;

            foreach (var item in Usable(inventory))
            {
                var row = Instantiate(itemPrefab, listContainer);
                row.Setup(item);
                row.OnChosen += HandleChosen;
                row.OnHighlighted += ShowDescription;
                selectables.Add(row.Button);
                if (firstItem == null) firstItem = item;
            }

            UINavigation.Vertical(selectables);
            firstSelected = selectables.Count > 0 ? selectables[0] : null;
            if (firstItem != null) ShowDescription(firstItem);
            Open();
        }

        private void ShowDescription(IItemInfo item)
        {
            if (descriptionText != null && item != null) descriptionText.text = item.Description;
        }

        private void HandleChosen(IItemInfo item)
        {
            Close();
            OnItemChosen?.Invoke(item);
        }
    }
}
