using System.Collections;
using UnityEngine;

namespace UniQuest.UI
{
    /// <summary>
    /// La grande boîte de texte en bas à gauche de l'écran de combat.
    ///  - Prompt("Que doit faire Trochu ?") : texte fixe pendant le choix d'action.
    ///  - yield return Say("Coup critique !") : message écrit lettre par lettre,
    ///    puis attente d'Entrée (ou avance auto si autoAdvanceDelay > 0).
    /// </summary>
    public class FightLog : MonoBehaviour
    {
        [SerializeField] private TypewriterText text;
        [SerializeField] private GameObject continueIndicator;
        [Tooltip("Optionnel : \"Flèches : naviguer · Entrée : valider · Échap : retour\"")]
        [SerializeField] private GameObject controlsHint;
        [Tooltip("0 = attendre Entrée. Sinon, le message avance tout seul après ce délai.")]
        [SerializeField] private float autoAdvanceDelay = 0f;

        public void Prompt(string message)
        {
            text.ShowInstant(message);
            if (continueIndicator != null) continueIndicator.SetActive(false);
            if (controlsHint != null) controlsHint.SetActive(true);
        }

        public IEnumerator Say(string message)
        {
            if (controlsHint != null) controlsHint.SetActive(false);
            if (continueIndicator != null) continueIndicator.SetActive(false);

            var typing = StartCoroutine(text.Play(message));
            yield return null;
            while (text.IsPlaying)
            {
                if (UIInput.TryConsumeConfirm()) text.Skip();
                yield return null;
            }
            StopCoroutine(typing);

            if (continueIndicator != null) continueIndicator.SetActive(true);
            yield return null;

            float t = 0f;
            while (true)
            {
                if (UIInput.TryConsumeConfirm()) break;
                t += Time.unscaledDeltaTime;
                if (autoAdvanceDelay > 0f && t >= autoAdvanceDelay) break;
                yield return null;
            }

            if (continueIndicator != null) continueIndicator.SetActive(false);
        }
    }
}
