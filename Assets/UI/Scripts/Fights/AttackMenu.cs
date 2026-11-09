using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Liste des attaques (écran 3) avec le panneau de détail à droite :
    /// puissance, précision, critique, type. Échap revient au menu d'action.
    /// Sert pour "Attaque" (attaques physiques) et "Magie" (attaques qui coûtent des PM).
    /// </summary>
    public class AttackMenu : MenuPanel
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Transform listContainer;
        [SerializeField] private AttackButton attackPrefab;

        [Header("Détail de l'attaque sélectionnée")]
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailPower;
        [SerializeField] private TMP_Text detailAccuracy;
        [SerializeField] private TMP_Text detailCrit;
        [SerializeField] private TMP_Text detailType;
        [SerializeField] private TMP_Text detailDescription;

        public event Action<IAttackInfo> OnAttackChosen;

        /// <summary>Attaques débloquées de ce type pour ce perso (utile pour griser "Magie").</summary>
        public static List<IAttackInfo> Available(ICombatant user, bool magic)
        {
            var list = new List<IAttackInfo>();
            foreach (var a in user.Attacks)
                if (a.IsMagic == magic && a.UnlockLevel <= user.Level) list.Add(a);
            return list;
        }

        public void Show(ICombatant user, bool magic)
        {
            if (titleText != null) titleText.text = (magic ? "Magie — " : "Attaques — ") + user.Name;

            UINavigation.ClearChildren(listContainer);
            var selectables = new List<Selectable>();
            Selectable first = null;
            IAttackInfo firstAttack = null;

            foreach (var attack in Available(user, magic))
            {
                var row = Instantiate(attackPrefab, listContainer);
                row.Setup(attack, user.CurrentMP);
                row.OnChosen += HandleChosen;
                row.OnHighlighted += ShowDetail;
                selectables.Add(row.Button);
                if (first == null && row.Button.interactable) { first = row.Button; firstAttack = attack; }
            }

            UINavigation.Vertical(selectables);
            firstSelected = first;
            if (firstAttack != null) ShowDetail(firstAttack);

            Open();
        }

        private void ShowDetail(IAttackInfo a)
        {
            if (a == null) return;
            if (detailName != null) detailName.text = a.Name;
            if (detailPower != null) detailPower.text = a.Power.ToString();
            if (detailAccuracy != null) detailAccuracy.text = $"{a.Accuracy} %";
            if (detailCrit != null) detailCrit.text = $"{a.CritChance} %";
            if (detailType != null) detailType.text = a.TypeName;
            if (detailDescription != null) detailDescription.text = a.Description;
        }

        private void HandleChosen(IAttackInfo attack)
        {
            Close();
            OnAttackChosen?.Invoke(attack);
        }
    }
}
