using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniQuest.UI
{

    /// Petite fenêtre Oui / Non : écraser une sauvegarde, jeter un objet, quitter.
        public class ConfirmPopup : MenuPanel
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TMP_Text yesLabel;
        [SerializeField] private TMP_Text noLabel;

        private Action onYes;
        private Action onNo;

        protected virtual void Awake()
        {
            yesButton.onClick.AddListener(() => Answer(true));
            noButton.onClick.AddListener(() => Answer(false));
        }

        public void Ask(string message, Action yes, Action no = null, string yesText = "Oui", string noText = "Non")
        {
            messageText.text = message;
            if (yesLabel != null) yesLabel.text = yesText;
            if (noLabel != null) noLabel.text = noText;
            onYes = yes;
            onNo = no;
            firstSelected = noButton;
            UINavigation.Horizontal(new Selectable[] { yesButton, noButton }, wrap: false);
            Open();
        }

        protected override void HandleCancel() => Answer(false);

        private void Answer(bool yes)
        {
            var callback = yes ? onYes : onNo;
            onYes = null;
            onNo = null;
            Close();
            callback?.Invoke();
        }
    }
}
