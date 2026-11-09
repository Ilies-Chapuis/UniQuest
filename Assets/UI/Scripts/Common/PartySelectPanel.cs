using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Panneau "choisis un membre de l'équipe". Un seul panneau pour trois usages :
    ///  - remplacer un perso KO en combat (pas d'annulation possible) ;
    ///  - changer de perso volontairement en combat ;
    ///  - choisir qui boit la potion / qui améliore son arme dans l'inventaire.
    /// </summary>
    public class PartySelectPanel : MenuPanel
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Transform listContainer;
        [SerializeField] private PartyMemberButton memberPrefab;

        private Action<ICombatant> onPicked;
        private Action onCancelled;
        private bool cancellable;

        /// <param name="canPick">Quels membres sont sélectionnables (ex. pas les KO).</param>
        /// <param name="active">Membre actuellement au combat, marqué visuellement (peut être null).</param>
        public void Show(string title, IReadOnlyList<ICombatant> members, Func<ICombatant, bool> canPick,
                         Action<ICombatant> picked, bool allowCancel, Action cancelled = null,
                         ICombatant active = null, string hint = null)
        {
            onPicked = picked;
            onCancelled = cancelled;
            cancellable = allowCancel;
            closeOnCancel = allowCancel;

            if (titleText != null) titleText.text = title;
            if (hintText != null)
            {
                hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
                hintText.text = hint;
            }

            UINavigation.ClearChildren(listContainer);
            var selectables = new List<Selectable>();
            Selectable first = null;

            foreach (var m in members)
            {
                var row = Instantiate(memberPrefab, listContainer);
                bool ok = canPick == null || canPick(m);
                row.Setup(m, ok, m == active);
                row.OnChosen += HandleChosen;
                selectables.Add(row.Button);
                if (ok && first == null) first = row.Button;
            }

            UINavigation.Vertical(selectables);
            firstSelected = first;

            if (IsOpen) SelectDefault();
            else Open();
        }

        private void HandleChosen(ICombatant member)
        {
            var callback = onPicked;
            onPicked = null;
            onCancelled = null;
            Close();
            callback?.Invoke(member);
        }

        protected override void HandleCancel()
        {
            if (!cancellable) return;
            var callback = onCancelled;
            onPicked = null;
            onCancelled = null;
            Close();
            callback?.Invoke();
        }
    }
}
