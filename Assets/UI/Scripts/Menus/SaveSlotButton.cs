using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Un emplacement de sauvegarde (écran 7) :
    /// "Emplacement 1 — Forêt des larves" / équipe / date, temps de jeu, seed.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SaveSlotButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text partyText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text detailsText;

        private Button button;
        private bool initialized;

        public SaveSlotInfo Info { get; private set; }
        public Button Button => button;

        public event Action<SaveSlotInfo> OnChosen;

        private void Init()
        {
            if (initialized) return;
            initialized = true;
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnChosen?.Invoke(Info));
        }

        private void Awake() => Init();

        public void Setup(SaveSlotInfo info, bool selectable)
        {
            Init();
            Info = info;
            int number = info.Index + 1;

            if (info.IsEmpty)
            {
                titleText.text = $"Emplacement {number} — vide";
                SetText(partyText, "");
                SetText(dateText, "");
                SetText(detailsText, "");
            }
            else
            {
                titleText.text = $"Emplacement {number} — {info.ZoneName}";
                SetText(partyText, info.PartySummary);
                SetText(dateText, info.SavedAt.ToString("dd/MM/yyyy HH:mm"));
                int hours = (int)info.PlayTime.TotalHours;
                SetText(detailsText, $"Temps de jeu {hours:00}:{info.PlayTime.Minutes:00} · Seed {info.Seed}");
            }

            var mb = GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(selectable);
            else button.interactable = selectable;
        }

        private static void SetText(TMP_Text t, string value)
        {
            if (t != null) t.text = value;
        }
    }
}
