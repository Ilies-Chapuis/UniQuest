using UnityEngine;

namespace UniQuest.UI
{
    /// <summary>
    /// Ouvre le menu pause avec Échap, sur la carte uniquement.
    /// À mettre sur un GameObject TOUJOURS ACTIF de la scène de la carte
    /// (le menu pause, lui, est désactivé tant qu'il est fermé : il ne peut pas écouter Échap).
    /// Ne le mets pas dans la scène de combat.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private PauseMenu pauseMenu;
        [Tooltip("Met le jeu en pause (Time.timeScale = 0) pendant que le menu est ouvert")]
        [SerializeField] private bool freezeTime = true;

        private void OnEnable() => pauseMenu.OnClosed += HandleClosed;
        private void OnDisable() => pauseMenu.OnClosed -= HandleClosed;

        private void Update()
        {
            // Pas de pause pendant un dialogue ou un autre menu
            if (UIState.BlocksGameplay || pauseMenu.IsOpen) return;

            if (UIInput.TryConsumePause())
            {
                pauseMenu.Open();
                if (freezeTime) Time.timeScale = 0f;
            }
        }

        private void HandleClosed()
        {
            if (freezeTime) Time.timeScale = 1f;
        }
    }
}
