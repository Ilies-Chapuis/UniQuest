using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniQuest.UI
{
    /// <summary>
    /// Petite notification en haut de l'écran : "Coffre ouvert : Potion ×1", "Partie sauvegardée".
    /// Elle n'interrompt pas le jeu. Plusieurs appels d'affilée sont mis en file d'attente.
    /// Montage : un panneau avec CanvasGroup, un texte et (optionnel) une icône. Laisse-le ACTIF.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationToast : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image icon;
        [SerializeField] private float displayTime = 2f;
        [SerializeField] private float fadeTime = 0.2f;

        private CanvasGroup group;
        private readonly Queue<KeyValuePair<string, Sprite>> pending = new Queue<KeyValuePair<string, Sprite>>();
        private Coroutine running;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public void Show(string message, Sprite iconSprite = null)
        {
            pending.Enqueue(new KeyValuePair<string, Sprite>(message, iconSprite));
            if (running == null) running = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (pending.Count > 0)
            {
                var next = pending.Dequeue();
                messageText.text = next.Key;
                if (icon != null)
                {
                    icon.sprite = next.Value;
                    icon.gameObject.SetActive(next.Value != null);
                }

                yield return Fade(0f, 1f);
                yield return new WaitForSecondsRealtime(displayTime);
                yield return Fade(1f, 0f);
            }
            running = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / fadeTime);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
