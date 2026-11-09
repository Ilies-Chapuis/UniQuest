using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UniQuest.UI
{
    /// <summary>
    /// Le menu pause : la barre d'onglets Équipe / Inventaire / Sauvegarde / Quitter
    /// (en haut des écrans 5, 6 et 7). Entrée sur un onglet ouvre son contenu,
    /// Échap dans le contenu revient aux onglets, Échap sur les onglets ferme le menu.
    ///
    /// Hiérarchie conseillée :
    ///   PauseMenu (ce script, désactivé)
    ///     ├─ Tabs (4 boutons avec MenuButton)
    ///     ├─ TeamMenu       (désactivé)
    ///     ├─ InventoryMenu  (désactivé)
    ///     └─ SaveMenu       (désactivé)
    /// </summary>
    public class PauseMenu : MenuPanel
    {
        [Header("Onglets")]
        [SerializeField] private Button teamTab;
        [SerializeField] private Button inventoryTab;
        [SerializeField] private Button saveTab;
        [SerializeField] private Button quitTab;

        [Header("Contenus")]
        [SerializeField] private TeamMenu teamMenu;
        [SerializeField] private InventoryMenu inventoryMenu;
        [SerializeField] private SaveMenu saveMenu;
        [SerializeField] private ConfirmPopup confirmPopup;

        [Header("Quitter")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        private bool listenersAdded;

        protected override void HandleOpened()
        {
            if (!listenersAdded)
            {
                listenersAdded = true;
                teamTab.onClick.AddListener(() => teamMenu.Open());
                inventoryTab.onClick.AddListener(() => inventoryMenu.Open());
                saveTab.onClick.AddListener(() => saveMenu.OpenForSave());
                quitTab.onClick.AddListener(AskQuit);
            }

            UINavigation.Horizontal(new Selectable[] { teamTab, inventoryTab, saveTab, quitTab });
            if (firstSelected == null) firstSelected = teamTab;
        }

        protected override void HandleClosing()
        {
            // Si on ferme le menu pause, on ferme aussi ce qui était ouvert dedans
            teamMenu.Close();
            inventoryMenu.Close();
            saveMenu.Close();
        }

        private void AskQuit()
        {
            confirmPopup.Ask("Retourner au menu principal ? La progression non sauvegardée sera perdue.",
                yes: () =>
                {
                    MenuPanel.CloseAll();
                    UIState.Reset();
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(mainMenuScene);
                },
                yesText: "Quitter", noText: "Rester");
        }
    }
}
