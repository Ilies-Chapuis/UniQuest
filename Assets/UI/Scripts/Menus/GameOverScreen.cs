using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UniQuest.UI
{
    /// <summary>
    /// Écran de Game Over : fondu au noir, texte, puis "Reprendre la dernière sauvegarde"
    /// ou "Menu principal". Ouvert par FightUI quand toute l'équipe est KO.
    /// </summary>
    public class GameOverScreen : MenuPanel
    {
        [Tooltip("Ce qui implémente ISaveService. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour saveSource;

        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 1.2f;
        [SerializeField] private TypewriterText messageText;
        [SerializeField] private string message = "Trochu est tombé. Les larves, elles, ont faim.";
        [SerializeField] private GameObject buttonsGroup;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuScene = "MainMenu";

        private Contracts.ISaveService saves;
        private bool listenersAdded;

        protected override void HandleOpened()
        {
            closeOnCancel = false;
            if (saves == null) saves = UIServices.Resolve<Contracts.ISaveService>(saveSource);

            if (!listenersAdded)
            {
                listenersAdded = true;
                retryButton.onClick.AddListener(Retry);
                mainMenuButton.onClick.AddListener(BackToMenu);
            }

            bool canRetry = saves != null && saves.LastUsedSlot >= 0;
            var mb = retryButton.GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(canRetry);
            else retryButton.interactable = canRetry;
            UINavigation.Vertical(new Selectable[] { retryButton, mainMenuButton });
            firstSelected = canRetry ? retryButton : mainMenuButton;

            if (buttonsGroup != null) buttonsGroup.SetActive(false);
            StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            if (fadeGroup != null)
            {
                for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
                {
                    fadeGroup.alpha = t / fadeDuration;
                    yield return null;
                }
                fadeGroup.alpha = 1f;
            }

            if (messageText != null) yield return messageText.Play(message);

            if (buttonsGroup != null) buttonsGroup.SetActive(true);
            SelectDefault();
        }

        private void Retry()
        {
            if (saves == null || saves.LastUsedSlot < 0) return;
            MenuPanel.CloseAll();
            UIState.Reset();
            saves.Load(saves.LastUsedSlot);
        }

        private void BackToMenu()
        {
            MenuPanel.CloseAll();
            UIState.Reset();
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
