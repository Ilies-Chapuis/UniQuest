using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Une ligne "membre de l'équipe" : nom, niveau, type, barre de PV.
    /// Réutilisée dans : l'écran de remplacement après KO, la liste du menu équipe,
    /// et le choix de la cible d'une potion.
    /// À mettre sur un prefab avec un Button + MenuButton.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PartyMemberButton : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private TMP_Text nameText;
        [Tooltip("Ex. \"Niv. 5 · Alchimie\" ou \"KO\"")]
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private StatBar hpBar;
        [Tooltip("Optionnel : petite icône du perso")]
        [SerializeField] private Image icon;
        [Tooltip("Optionnel : marqueur \"au combat\"")]
        [SerializeField] private GameObject activeMarker;

        private Button button;
        private MenuButton menuButton;

        public ICombatant Member { get; private set; }
        public Button Button => button;

        public event Action<ICombatant> OnChosen;
        /// <summary>Déclenché quand le bouton est sélectionné au clavier (pour afficher le détail).</summary>
        public event Action<ICombatant> OnHighlighted;

        private void Awake()
        {
            if (button != null) return;   // déjà initialisé par Setup() avant l'activation
            button = GetComponent<Button>();
            menuButton = GetComponent<MenuButton>();
            button.onClick.AddListener(() => OnChosen?.Invoke(Member));
        }

        public void Setup(ICombatant member, bool selectable, bool isActive = false)
        {
            if (button == null) Awake();
            Member = member;

            nameText.text = member.Name;
            if (infoText != null)
                infoText.text = member.IsKO ? "KO" : $"Niv. {member.Level} · {member.TypeName}";
            if (hpBar != null) hpBar.SetValue(member.CurrentHP, member.MaxHP, instant: true);
            if (icon != null)
            {
                icon.sprite = member.Portrait != null ? member.Portrait : member.BattleSprite;
                icon.enabled = icon.sprite != null;
                icon.preserveAspect = true;
            }
            if (activeMarker != null) activeMarker.SetActive(isActive);

            if (menuButton != null) menuButton.SetInteractable(selectable);
            else button.interactable = selectable;
        }

        public void OnSelect(BaseEventData eventData) => OnHighlighted?.Invoke(Member);
    }
}
