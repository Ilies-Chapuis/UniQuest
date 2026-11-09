using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace UniQuest.UI
{
    /// <summary>
    /// Menu principal : Nouvelle partie / Continuer / Charger / Quitter.
    /// Le champ de seed (optionnel) permet de rejouer exactement la même partie :
    /// pratique en soutenance pour montrer le système de seed demandé par le sujet.
    ///
    /// Le gameplay lit la seed choisie dans MainMenu.RequestedSeed (null = seed aléatoire).
    /// </summary>
    public class MainMenu : MenuPanel
    {
        [Tooltip("Ce qui implémente ISaveService. Vide = recherche automatique.")]
        [SerializeField] private MonoBehaviour saveSource;

        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private SaveMenu saveMenu;

        [Header("Nouvelle partie")]
        [SerializeField] private string firstScene = "Overworld";
        [Tooltip("Optionnel : champ texte pour entrer une seed")]
        [SerializeField] private TMP_InputField seedInput;

        /// <summary>Seed demandée par le joueur, ou null pour une seed aléatoire.</summary>
        public static int? RequestedSeed { get; private set; }

        private Contracts.ISaveService saves;

        private void Start()
        {
            UIState.Reset();
            Time.timeScale = 1f;
            closeOnCancel = false;

            saves = UIServices.Resolve<Contracts.ISaveService>(saveSource);

            newGameButton.onClick.AddListener(NewGame);
            continueButton.onClick.AddListener(Continue);
            loadButton.onClick.AddListener(() => saveMenu.OpenForLoad());
            quitButton.onClick.AddListener(Quit);

            bool hasSave = saves != null && saves.HasAnySave;
            SetInteractable(continueButton, hasSave && saves.LastUsedSlot >= 0);
            SetInteractable(loadButton, hasSave);

            UINavigation.Vertical(new Selectable[] { newGameButton, continueButton, loadButton, quitButton });
            firstSelected = hasSave ? continueButton : newGameButton;
            Open();
        }

        private void NewGame()
        {
            RequestedSeed = null;
            if (seedInput != null && int.TryParse(seedInput.text, out int seed)) RequestedSeed = seed;
            SceneManager.LoadScene(firstScene);
        }

        private void Continue()
        {
            if (saves != null && saves.LastUsedSlot >= 0) saves.Load(saves.LastUsedSlot);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void SetInteractable(Button b, bool value)
        {
            var mb = b.GetComponent<MenuButton>();
            if (mb != null) mb.SetInteractable(value);
            else b.interactable = value;
        }
    }
}
