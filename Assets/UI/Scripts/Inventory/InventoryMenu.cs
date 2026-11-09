using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Inventaire (écran 6) : grille d'objets filtrée par catégorie, fiche de l'objet à droite.
    ///  - Q / E : catégorie précédente / suivante (Tout, Soins, Boosts, Matériaux, Clés, Snacks)
    ///  - Entrée sur un objet : ouvre ses actions (Utiliser sur… / Améliorer l'arme / Jeter)
    ///  - Échap : revient à la grille, puis aux onglets du menu pause
    /// </summary>
    public class InventoryMenu : MenuPanel
    {
        [Tooltip("Ce qui implémente IInventory. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour inventorySource;
        [Tooltip("Ce qui implémente IParty. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour partySource;
        [SerializeField] private UITheme theme;

        [Header("Catégories (dans l'ordre : Tout, Soins, Boosts, Matériaux, Clés, Snacks)")]
        [SerializeField] private TMP_Text[] categoryLabels;
        [Tooltip("Optionnel : soulignement de l'onglet actif, un par libellé")]
        [SerializeField] private GameObject[] categoryUnderlines;

        [Header("Grille")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private ItemRowButton slotPrefab;
        [SerializeField] private int columns = 4;
        [SerializeField] private GameObject emptyMessage;

        [Header("Fiche de l'objet")]
        [SerializeField] private GameObject detailGroup;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailCategory;
        [SerializeField] private TMP_Text detailDescription;

        [Header("Actions sur l'objet")]
        [Tooltip("Petit MenuPanel contenant les deux boutons ci-dessous")]
        [SerializeField] private MenuPanel actionsPanel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryLabel;
        [SerializeField] private Button discardButton;

        [Header("Panneaux partagés")]
        [SerializeField] private PartySelectPanel partySelect;
        [SerializeField] private ConfirmPopup confirmPopup;
        [SerializeField] private NotificationToast toast;

        // null = "Tout"
        private static readonly ItemCategory?[] Categories =
        {
            null, ItemCategory.Heal, ItemCategory.Boost, ItemCategory.Material, ItemCategory.Key, ItemCategory.Snack
        };

        private IInventory inventory;
        private IParty party;
        private int categoryIndex;
        private IItemInfo current;
        private bool listenersAdded;

        protected override void HandleOpened()
        {
            if (inventory == null) inventory = UIServices.Resolve<IInventory>(inventorySource);
            if (party == null) party = UIServices.Resolve<IParty>(partySource);
            if (inventory == null) { Debug.LogWarning("[InventoryMenu] Aucun IInventory trouvé.", this); return; }

            if (!listenersAdded)
            {
                listenersAdded = true;
                primaryButton.onClick.AddListener(UsePrimary);
                discardButton.onClick.AddListener(AskDiscard);
            }
            inventory.OnInventoryChanged += Rebuild;
            Rebuild();
        }

        protected override void HandleClosing()
        {
            if (inventory != null) inventory.OnInventoryChanged -= Rebuild;
            if (actionsPanel != null) actionsPanel.Close();
        }

        protected override void Update()
        {
            base.Update();
            if (!IsOpen || !IsTopmost) return;

            if (UIInput.PreviousTabPressed) ChangeCategory(-1);
            else if (UIInput.NextTabPressed) ChangeCategory(+1);
        }

        private void ChangeCategory(int delta)
        {
            categoryIndex = (categoryIndex + delta + Categories.Length) % Categories.Length;
            Rebuild();
            SelectDefault();
        }

        // ───────────────────────── Affichage ─────────────────────────

        private void Rebuild()
        {
            RefreshCategoryLabels();

            UINavigation.ClearChildren(gridContainer);
            var selectables = new List<Selectable>();
            IItemInfo first = null;
            var filter = Categories[categoryIndex];

            foreach (var item in inventory.Items)
            {
                if (item.Quantity <= 0) continue;
                if (filter.HasValue && item.Category != filter.Value) continue;

                var slot = Instantiate(slotPrefab, gridContainer);
                slot.Setup(item);
                slot.OnHighlighted += ShowDetail;
                slot.OnChosen += OpenActions;
                selectables.Add(slot.Button);
                if (first == null) first = item;
            }

            UINavigation.Grid(selectables, columns);
            firstSelected = selectables.Count > 0 ? selectables[0] : null;

            if (emptyMessage != null) emptyMessage.SetActive(first == null);
            if (detailGroup != null) detailGroup.SetActive(first != null);
            if (first != null) ShowDetail(first);
            if (IsOpen && IsTopmost) SelectDefault();
        }

        private void RefreshCategoryLabels()
        {
            Color accent = theme != null ? theme.accent : (Color)new Color32(0xE8, 0xB5, 0x4A, 0xFF);
            Color text = theme != null ? theme.text : (Color)new Color32(0xEC, 0xE0, 0xCF, 0xFF);
            for (int i = 0; categoryLabels != null && i < categoryLabels.Length; i++)
                if (categoryLabels[i] != null) categoryLabels[i].color = i == categoryIndex ? accent : text;
            for (int i = 0; categoryUnderlines != null && i < categoryUnderlines.Length; i++)
                if (categoryUnderlines[i] != null) categoryUnderlines[i].SetActive(i == categoryIndex);
        }

        private void ShowDetail(IItemInfo item)
        {
            if (item == null) return;
            current = item;
            if (detailName != null) detailName.text = item.Name;
            if (detailCategory != null) detailCategory.text = $"{CategoryName(item.Category)} · ×{item.Quantity}";
            if (detailDescription != null) detailDescription.text = item.Description;
            if (detailIcon != null)
            {
                detailIcon.sprite = item.Icon;
                detailIcon.enabled = item.Icon != null;
                detailIcon.preserveAspect = true;
            }
        }

        public static string CategoryName(ItemCategory c)
        {
            switch (c)
            {
                case ItemCategory.Heal: return "Soins";
                case ItemCategory.Boost: return "Boost";
                case ItemCategory.Material: return "Matériau";
                case ItemCategory.Key: return "Objet clé";
                case ItemCategory.Snack: return "Snack";
                default: return c.ToString();
            }
        }

        // ───────────────────────── Actions ─────────────────────────

        private void OpenActions(IItemInfo item)
        {
            current = item;
            bool isMaterial = item.Category == ItemCategory.Material;
            bool isKey = item.Category == ItemCategory.Key;

            if (isMaterial)
            {
                primaryLabel.text = "Améliorer l'arme";
                SetInteractable(primaryButton, HasUpgradeableMember());
            }
            else if (isKey)
            {
                primaryLabel.text = "Objet clé";
                SetInteractable(primaryButton, false);
            }
            else
            {
                primaryLabel.text = item.NeedsTarget ? "Utiliser sur…" : "Utiliser";
                SetInteractable(primaryButton, item.UsableOutsideFight);
            }
            SetInteractable(discardButton, !isKey);   // on ne jette pas une clé

            UINavigation.Vertical(new Selectable[] { primaryButton, discardButton }, wrap: false);
            actionsPanel.Open();
        }

        private void UsePrimary()
        {
            if (current == null) return;
            var item = current;

            if (item.Category == ItemCategory.Material)
            {
                partySelect.Show("Améliorer l'arme de qui ?", party.Members,
                    canPick: m => m.Weapon != null && inventory.CanUpgradeWeapon(m),
                    picked: m =>
                    {
                        string cost = inventory.GetUpgradeCost(m);
                        if (inventory.UpgradeWeapon(m))
                            Notify($"{m.Weapon.Name} passe à +{m.Weapon.UpgradeLevel} ! ({cost})");
                        actionsPanel.Close();
                    },
                    allowCancel: true);
                return;
            }

            if (!item.NeedsTarget)
            {
                if (inventory.Use(item, null)) Notify($"{item.Name} utilisé.");
                actionsPanel.Close();
                return;
            }

            partySelect.Show($"Utiliser {item.Name} sur qui ?", party.Members,
                canPick: m => !m.IsKO,
                picked: m =>
                {
                    if (inventory.Use(item, m)) Notify($"{m.Name} utilise {item.Name}.");
                    actionsPanel.Close();
                },
                allowCancel: true);
        }

        private void AskDiscard()
        {
            if (current == null) return;
            var item = current;
            confirmPopup.Ask($"Jeter {item.Name} ?", yes: () =>
            {
                inventory.Discard(item);
                actionsPanel.Close();
            }, yesText: "Jeter", noText: "Garder");
        }

        private bool HasUpgradeableMember()
        {
            if (party == null) return false;
            foreach (var m in party.Members)
                if (m.Weapon != null && inventory.CanUpgradeWeapon(m)) return true;
            return false;
        }

        private void Notify(string message)
        {
            if (toast != null) toast.Show(message);
        }

        private static void SetInteractable(Button b, bool value)
        {
            var mb = b.GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(value);
            else b.interactable = value;
        }
    }
}
