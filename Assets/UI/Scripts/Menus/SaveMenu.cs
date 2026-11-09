using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Sauvegarde / chargement (écran 7).
    ///  - Depuis le menu pause : OpenForSave() (Q / E pour basculer vers "Charger").
    ///  - Depuis le menu principal ou le Game Over : OpenForLoad(), sans possibilité de sauvegarder.
    /// Écraser une sauvegarde existante demande confirmation.
    /// </summary>
    public class SaveMenu : MenuPanel
    {
        [Tooltip("Ce qui implémente ISaveService. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour saveSource;
        [SerializeField] private UITheme theme;

        [Header("Mode")]
        [SerializeField] private TMP_Text saveModeLabel;
        [SerializeField] private TMP_Text loadModeLabel;
        [Tooltip("Optionnel : \"Q / E : changer de mode\"")]
        [SerializeField] private GameObject modeHint;

        [Header("Emplacements")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private SaveSlotButton slotPrefab;

        [Header("Panneaux partagés")]
        [SerializeField] private ConfirmPopup confirmPopup;
        [SerializeField] private NotificationToast toast;

        private ISaveService saves;
        private bool saveMode;
        private bool canSwitchMode;

        public void OpenForSave()
        {
            saveMode = true;
            canSwitchMode = true;
            Open();
        }

        public void OpenForLoad()
        {
            saveMode = false;
            canSwitchMode = false;
            Open();
        }

        protected override void HandleOpened()
        {
            if (saves == null) saves = UIServices.Resolve<ISaveService>(saveSource);
            if (saves == null) { Debug.LogWarning("[SaveMenu] Aucun ISaveService trouvé.", this); return; }
            Rebuild();
        }

        protected override void Update()
        {
            base.Update();
            if (!IsOpen || !IsTopmost || !canSwitchMode) return;
            if (UIInput.PreviousTabPressed || UIInput.NextTabPressed)
            {
                saveMode = !saveMode;
                Rebuild();
                SelectDefault();
            }
        }

        private void Rebuild()
        {
            Color accent = theme != null ? theme.accent : (Color)new Color32(0xE8, 0xB5, 0x4A, 0xFF);
            Color text = theme != null ? theme.textMuted : (Color)new Color32(0xC4, 0xB6, 0xA4, 0xFF);
            if (saveModeLabel != null)
            {
                saveModeLabel.gameObject.SetActive(canSwitchMode);
                saveModeLabel.color = saveMode ? accent : text;
            }
            if (loadModeLabel != null) loadModeLabel.color = saveMode ? text : accent;
            if (modeHint != null) modeHint.SetActive(canSwitchMode);

            UINavigation.ClearChildren(slotContainer);
            var selectables = new List<Selectable>();
            foreach (var info in saves.GetSlots())
            {
                var slot = Instantiate(slotPrefab, slotContainer);
                // En chargement, un emplacement vide n'est pas sélectionnable
                slot.Setup(info, selectable: saveMode || !info.IsEmpty);
                slot.OnChosen += HandleChosen;
                selectables.Add(slot.Button);
            }
            UINavigation.Vertical(selectables);
            firstSelected = null;   // premier bouton actif
            foreach (var s in selectables)
                if (s.interactable) { firstSelected = s; break; }
        }

        private void HandleChosen(SaveSlotInfo info)
        {
            if (saveMode)
            {
                if (info.IsEmpty) DoSave(info.Index);
                else confirmPopup.Ask($"Écraser l'emplacement {info.Index + 1} ?",
                                      yes: () => DoSave(info.Index), yesText: "Écraser", noText: "Annuler");
            }
            else
            {
                confirmPopup.Ask($"Charger l'emplacement {info.Index + 1} ?",
                                 yes: () => DoLoad(info.Index), yesText: "Charger", noText: "Annuler");
            }
        }

        private void DoSave(int index)
        {
            bool ok = saves.Save(index);
            if (toast != null) toast.Show(ok ? "Partie sauvegardée." : "La sauvegarde a échoué.");
            Rebuild();
            SelectDefault();
        }

        private void DoLoad(int index)
        {
            // Le gameplay charge la scène : on referme tous les menus avant
            MenuPanel.CloseAll();
            UIState.Reset();
            Time.timeScale = 1f;
            if (!saves.Load(index) && toast != null) toast.Show("Impossible de charger cette partie.");
        }
    }
}
