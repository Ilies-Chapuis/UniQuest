using System.Collections;
using UnityEngine;
using TMPro;

namespace UniQuest.UI
{
    /// <summary>
    /// Affiche un texte lettre par lettre (boîte de dialogue, messages de combat).
    /// Utilisation dans une coroutine :
    ///     yield return typewriter.Play("La larve hurle !");
    /// Skip() affiche tout d'un coup (le joueur appuie sur Entrée pendant l'écriture).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField] private float charactersPerSecond = 45f;
        [Tooltip("Pause supplémentaire après . ! ? (en secondes)")]
        [SerializeField] private float punctuationPause = 0.12f;
        [Tooltip("Optionnel : petit son joué toutes les N lettres")]
        [SerializeField] private AudioSource blipSource;
        [SerializeField] private int blipEveryNChars = 3;

        private TMP_Text text;
        private bool skipRequested;

        public bool IsPlaying { get; private set; }

        private void Awake() => text = GetComponent<TMP_Text>();

        public IEnumerator Play(string content)
        {
            if (text == null) text = GetComponent<TMP_Text>();

            IsPlaying = true;
            skipRequested = false;
            text.text = content;
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();   // nécessaire pour connaître le nombre réel de caractères (balises exclues)

            int total = text.textInfo.characterCount;
            float delay = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;

            for (int i = 1; i <= total; i++)
            {
                if (skipRequested) break;

                text.maxVisibleCharacters = i;

                if (blipSource != null && blipEveryNChars > 0 && i % blipEveryNChars == 0)
                    blipSource.Play();

                char c = text.textInfo.characterInfo[i - 1].character;
                float wait = delay + ((c == '.' || c == '!' || c == '?') ? punctuationPause : 0f);
                if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
            }

            text.maxVisibleCharacters = int.MaxValue;
            IsPlaying = false;
        }

        public void Skip() => skipRequested = true;

        public void ShowInstant(string content)
        {
            if (text == null) text = GetComponent<TMP_Text>();
            text.text = content;
            text.maxVisibleCharacters = int.MaxValue;
            IsPlaying = false;
        }
    }
}
