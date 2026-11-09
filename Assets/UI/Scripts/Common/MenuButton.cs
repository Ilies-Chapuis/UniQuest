using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace UniQuest.UI
{
    /// À mettre sur chaque bouton de menu, à côté du composant Button.

    [RequireComponent(typeof(Selectable))]
    public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, ISubmitHandler, IPointerClickHandler
    {
        [SerializeField] private UITheme theme;
        [Tooltip("Textes recolorés (le libellé, le coût en PM...)")]
        [SerializeField] private TMP_Text[] labels;
        [Tooltip("Optionnel : fond affiché seulement quand le bouton est sélectionné")]
        [SerializeField] private Graphic selectedBackground;
        [Tooltip("Optionnel : contour affiché seulement quand le bouton est sélectionné")]
        [SerializeField] private Graphic selectedOutline;

        private Selectable selectable;
        private bool isSelected;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            // On gère l'apparence nous-mêmes : on coupe la transition de couleur par défaut d'Unity
            selectable.transition = Selectable.Transition.None;
        }

        private void OnEnable() => Refresh();

        public void OnSelect(BaseEventData eventData) { isSelected = true; Refresh(); }
        public void OnDeselect(BaseEventData eventData) { isSelected = false; Refresh(); }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null && selectable.interactable && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        // Le bouton a "consommé" Entrée : un panneau qui s'ouvre ne doit pas la recevoir aussi
        public void OnSubmit(BaseEventData eventData) => UIInput.MarkConfirmUsed();
        public void OnPointerClick(PointerEventData eventData) => UIInput.MarkConfirmUsed();

        /// Active/désactive le bouton ET met à jour son apparence.
        public void SetInteractable(bool value)
        {
            if (selectable == null) selectable = GetComponent<Selectable>();
            selectable.interactable = value;
            Refresh();
        }

        public void Refresh()
        {
            if (selectable == null) selectable = GetComponent<Selectable>();
            bool enabled = selectable.interactable;

            Color text = theme != null ? theme.text : (Color)new Color32(0xEC, 0xE0, 0xCF, 0xFF);
            Color accent = theme != null ? theme.accent : (Color)new Color32(0xE8, 0xB5, 0x4A, 0xFF);
            Color disabled = theme != null ? theme.textDisabled : (Color)new Color32(0x8F, 0x84, 0x8A, 0xFF);

            Color c = !enabled ? disabled : (isSelected ? accent : text);
            if (labels != null)
                foreach (var l in labels)
                    if (l != null) l.color = c;

            if (selectedBackground != null) selectedBackground.enabled = isSelected;
            if (selectedOutline != null)
            {
                selectedOutline.enabled = isSelected;
                selectedOutline.color = accent;
            }
        }
    }
}
