using System;

namespace UniQuest.UI
{
    /// <summary>
    /// Dit au reste du jeu si l'UI "prend la main" (menu ouvert, dialogue en cours).
    /// Le PlayerController doit ignorer les flèches quand UIState.BlocksGameplay est vrai :
    ///     if (UIState.BlocksGameplay) return;
    /// C'est un compteur : deux panneaux ouverts l'un sur l'autre ne se "débloquent" pas mutuellement.
    /// </summary>
    public static class UIState
    {
        private static int blockers;

        public static bool BlocksGameplay => blockers > 0;

        public static event Action<bool> OnBlockingChanged;

        public static void Push()
        {
            blockers++;
            if (blockers == 1) OnBlockingChanged?.Invoke(true);
        }

        public static void Pop()
        {
            if (blockers == 0) return;
            blockers--;
            if (blockers == 0) OnBlockingChanged?.Invoke(false);
        }

        /// <summary>À appeler au chargement d'une scène (menu principal, game over...).</summary>
        public static void Reset()
        {
            bool was = blockers > 0;
            blockers = 0;
            if (was) OnBlockingChanged?.Invoke(false);
        }
    }
}
