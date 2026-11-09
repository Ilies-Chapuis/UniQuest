using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniQuest.UI
{
    /// Base de tous les panneaux (menus, sous-menus, popups).

    public class MenuPanel : MonoBehaviour
    {
        private static readonly List<MenuPanel> openStack = new List<MenuPanel>();

        [Tooltip("Bouton sélectionné à l'ouverture. Vide = premier bouton actif trouvé dans le panneau.")]
        [SerializeField] protected Selectable firstSelected;
        [Tooltip("Empêche le joueur de bouger sur la carte tant que le panneau est ouvert")]
        [SerializeField] private bool blocksGameplay = true;
        [Tooltip("Échap ferme ce panneau")]
        [SerializeField] protected bool closeOnCancel = true;

        private GameObject selectionBeforeOpen;
        private GameObject lastSelectedInside;

        public bool IsOpen { get; private set; }
        public bool IsTopmost => openStack.Count > 0 && openStack[openStack.Count - 1] == this;
        protected int OpenedFrame { get; private set; } = -1;

        public event Action OnOpened;
        public event Action OnClosed;

        public static bool AnyOpen => openStack.Count > 0;

        public void Open()
        {
            if (IsOpen) return;

            selectionBeforeOpen = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            gameObject.SetActive(true);
            IsOpen = true;
            OpenedFrame = Time.frameCount;
            openStack.Add(this);
            if (blocksGameplay) UIState.Push();

            HandleOpened();
            SelectDefault();
            OnOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;

            HandleClosing();
            IsOpen = false;
            openStack.Remove(this);
            if (blocksGameplay) UIState.Pop();
            gameObject.SetActive(false);

            // On rend la main au bouton qui avait ouvert ce panneau
            if (EventSystem.current != null && selectionBeforeOpen != null && selectionBeforeOpen.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(selectionBeforeOpen);

            OnClosed?.Invoke();
        }

        /// Appelé juste après l'ouverture : remplis le panneau ici.
        protected virtual void HandleOpened() { }

        /// Appelé juste avant la fermeture.
        protected virtual void HandleClosing() { }

        /// Appelé quand Échap est pressé sur ce panneau. Par défaut : fermer.
        protected virtual void HandleCancel() => Close();

        /// Sélectionne le bouton par défaut (ou le dernier sélectionné dans ce panneau).
        public void SelectDefault()
        {
            if (EventSystem.current == null) return;

            GameObject target = null;
            if (firstSelected != null && firstSelected.gameObject.activeInHierarchy && firstSelected.interactable)
                target = firstSelected.gameObject;
            else
                target = FindFirstSelectable();

            Select(target);
        }

        public void Select(GameObject target)
        {
            if (EventSystem.current == null) return;
            // Le passage par null force Unity à rafraîchir l'état visuel du bouton
            EventSystem.current.SetSelectedGameObject(null);
            if (target != null) EventSystem.current.SetSelectedGameObject(target);
        }

        protected GameObject FindFirstSelectable()
        {
            foreach (var s in GetComponentsInChildren<Selectable>())
                if (s.interactable && s.gameObject.activeInHierarchy && s.navigation.mode != Navigation.Mode.None)
                    return s.gameObject;
            return null;
        }

        protected virtual void Update()
        {
            if (!IsOpen || !IsTopmost) return;

            KeepSelectionAlive();

            if (closeOnCancel && Time.frameCount != OpenedFrame && UIInput.TryConsumeCancel())
                HandleCancel();
        }


        /// Un clic de souris dans le vide désélectionne tout, et le clavier ne marche plus.

        private void KeepSelectionAlive()
        {
            var es = EventSystem.current;
            if (es == null) return;

            var current = es.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy && current.transform.IsChildOf(transform))
            {
                lastSelectedInside = current;
                return;
            }

            // Rien de sélectionné, ou sélection "échappée" dans un autre panneau : on la ramène ici
            if (lastSelectedInside != null && lastSelectedInside.activeInHierarchy
                && lastSelectedInside.transform.IsChildOf(transform))
                Select(lastSelectedInside);
            else
                SelectDefault();
        }

        protected virtual void OnDisable()
        {
            // Si le panneau est désactivé autrement que par Close() (changement de scène...), on nettoie
            if (IsOpen)
            {
                IsOpen = false;
                openStack.Remove(this);
                if (blocksGameplay) UIState.Pop();
            }
        }

        /// Ferme tout (changement de scène, game over).
        public static void CloseAll()
        {
            for (int i = openStack.Count - 1; i >= 0; i--)
                if (openStack[i] != null) openStack[i].Close();
            openStack.Clear();
        }
    }
}
