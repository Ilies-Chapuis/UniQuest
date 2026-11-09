using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// Un objet affiché dans une liste ou une grille : icône, nom, quantité.

    [RequireComponent(typeof(Button))]
    public class ItemRowButton : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text quantityText;

        private Button button;
        private bool initialized;

        public IItemInfo Item { get; private set; }
        public Button Button => button;

        public event Action<IItemInfo> OnChosen;
        public event Action<IItemInfo> OnHighlighted;

        private void Init()
        {
            if (initialized) return;
            initialized = true;
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnChosen?.Invoke(Item));
        }

        private void Awake() => Init();

        public void Setup(IItemInfo item, bool selectable = true)
        {
            Init();
            Item = item;
            if (nameText != null) nameText.text = item.Name;
            if (quantityText != null) quantityText.text = $"×{item.Quantity}";
            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
                icon.preserveAspect = true;
            }

            var mb = GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(selectable);
            else button.interactable = selectable;
        }

        public void OnSelect(BaseEventData eventData) => OnHighlighted?.Invoke(Item);
    }
}
