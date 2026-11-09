using System;
using UnityEngine;
using UnityEngine.UI;

namespace UniQuest.UI
{
    /// <summary>
    /// Le menu Attaque / Magie / Objet / Équipe / Fuir (écran 2).
    /// Il ne fait que prévenir FightUI du choix ; il ne se ferme pas avec Échap
    /// (on ne peut pas "annuler" son tour).
    /// </summary>
    public class ActionMenu : MenuPanel
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button magicButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button teamButton;
        [SerializeField] private Button fleeButton;
        [Tooltip("Nombre de colonnes du menu (2 dans la maquette)")]
        [SerializeField] private int columns = 2;

        public event Action OnAttack;
        public event Action OnMagic;
        public event Action OnItem;
        public event Action OnTeam;
        public event Action OnFlee;

        private bool listenersAdded;

        private void AddListeners()
        {
            if (listenersAdded) return;
            listenersAdded = true;
            attackButton.onClick.AddListener(() => OnAttack?.Invoke());
            magicButton.onClick.AddListener(() => OnMagic?.Invoke());
            itemButton.onClick.AddListener(() => OnItem?.Invoke());
            teamButton.onClick.AddListener(() => OnTeam?.Invoke());
            fleeButton.onClick.AddListener(() => OnFlee?.Invoke());
        }

        /// <summary>Ouvre le menu en grisant ce qui n'est pas possible ce tour-ci.</summary>
        public void Show(bool canUseMagic, bool hasItems, bool canSwitch, bool canFlee)
        {
            AddListeners();
            closeOnCancel = false;

            SetEnabled(magicButton, canUseMagic);
            SetEnabled(itemButton, hasItems);
            SetEnabled(teamButton, canSwitch);
            SetEnabled(fleeButton, canFlee);

            UINavigation.Grid(new Selectable[] { attackButton, magicButton, itemButton, teamButton, fleeButton }, columns);
            firstSelected = attackButton;

            if (IsOpen) SelectDefault();
            else Open();
        }

        private static void SetEnabled(Button b, bool value)
        {
            var mb = b.GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(value);
            else b.interactable = value;
        }
    }
}
