using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UniQuest.UI
{
    /// <summary>
    /// Les touches lues directement par l'UI (hors navigation, gérée par l'EventSystem).
    /// Marche avec le nouvel Input System (par défaut dans Unity 6) et avec l'ancien.
    /// </summary>
    public static class UIInput
    {
        // Une touche ne doit agir qu'UNE fois par frame : sinon Échap ferme le sous-menu
        // ET le menu en dessous dans la même frame. Les "TryConsume" règlent ça.
        private static int lastBackFrame = -1;
        private static int lastConfirmFrame = -1;

        public static bool TryConsumeCancel()
        {
            if (!CancelPressed || lastBackFrame == Time.frameCount) return false;
            lastBackFrame = Time.frameCount;
            return true;
        }

        public static bool TryConsumePause()
        {
            if (!PausePressed || lastBackFrame == Time.frameCount) return false;
            lastBackFrame = Time.frameCount;
            return true;
        }

        public static bool TryConsumeConfirm()
        {
            if (!ConfirmPressed || lastConfirmFrame == Time.frameCount) return false;
            lastConfirmFrame = Time.frameCount;
            return true;
        }

        /// <summary>Marque la touche de validation comme utilisée (ex. un bouton vient de la recevoir).</summary>
        public static void MarkConfirmUsed() => lastConfirmFrame = Time.frameCount;

        /// <summary>Entrée, Espace ou Z : valider / passer un message.</summary>
        public static bool ConfirmPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame
                                     || k.spaceKey.wasPressedThisFrame || k.zKey.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                       || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z);
#endif
            }
        }

        /// <summary>Échap, Retour arrière ou X : revenir en arrière.</summary>
        public static bool CancelPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                return k != null && (k.escapeKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame
                                     || k.xKey.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)
                       || Input.GetKeyDown(KeyCode.X);
#endif
            }
        }

        /// <summary>Q ou Page précédente : onglet précédent (catégories d'inventaire...).</summary>
        public static bool PreviousTabPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                return k != null && (k.qKey.wasPressedThisFrame || k.pageUpKey.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.PageUp);
#endif
            }
        }

        /// <summary>E ou Page suivante : onglet suivant.</summary>
        public static bool NextTabPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                return k != null && (k.eKey.wasPressedThisFrame || k.pageDownKey.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.PageDown);
#endif
            }
        }

        /// <summary>Échap seul : ouvrir le menu pause.</summary>
        public static bool PausePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                return k != null && k.escapeKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.Escape);
#endif
            }
        }
    }
}
