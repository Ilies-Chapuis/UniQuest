using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UniQuest.UI
{
    /// <summary>
    /// Boîte de dialogue des PNJ (écran 1 des maquettes).
    /// Appel depuis le script d'un PNJ :
    ///     dialogueBox.Show("Vieil homme", new[] { "Ligne 1", "Ligne 2" }, () => Debug.Log("fini"));
    /// Entrée pendant l'écriture : affiche toute la ligne. Entrée ensuite : ligne suivante.
    /// Le joueur ne peut pas bouger tant que la boîte est ouverte.
    /// </summary>
    public class DialogueBox : MenuPanel
    {
        [SerializeField] private TMP_Text speakerText;
        [Tooltip("Le cadre du nom, masqué quand il n'y a pas de nom (pancarte, narration)")]
        [SerializeField] private GameObject speakerFrame;
        [SerializeField] private TypewriterText lineText;
        [Tooltip("Le petit triangle qui clignote en bas à droite")]
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private float blinkInterval = 0.4f;

        private readonly Queue<string> lines = new Queue<string>();
        private Action onFinished;
        private Coroutine running;

        public bool IsBusy => IsOpen;

        public void Show(string speaker, IEnumerable<string> dialogueLines, Action finished = null)
        {
            if (IsOpen) return;   // un seul dialogue à la fois

            lines.Clear();
            foreach (var l in dialogueLines) lines.Enqueue(l);
            if (lines.Count == 0) { finished?.Invoke(); return; }

            onFinished = finished;
            closeOnCancel = false;

            bool hasSpeaker = !string.IsNullOrEmpty(speaker);
            if (speakerFrame != null) speakerFrame.SetActive(hasSpeaker);
            if (speakerText != null) speakerText.text = speaker;

            Open();
            running = StartCoroutine(Run());
        }

        /// <summary>Raccourci pour une seule ligne (pancarte, coffre vide...).</summary>
        public void Show(string speaker, string line, Action finished = null)
        {
            Show(speaker, new[] { line }, finished);
        }

        private IEnumerator Run()
        {
            while (lines.Count > 0)
            {
                if (continueIndicator != null) continueIndicator.SetActive(false);

                var typing = StartCoroutine(lineText.Play(lines.Dequeue()));
                // On ignore la touche qui a ouvert le dialogue (sinon la 1re ligne est sautée)
                yield return null;

                while (lineText.IsPlaying)
                {
                    if (UIInput.TryConsumeConfirm()) lineText.Skip();
                    yield return null;
                }
                StopCoroutine(typing);

                // Attente de la touche, avec le triangle qui clignote
                float t = 0f;
                yield return null;
                while (!UIInput.TryConsumeConfirm())
                {
                    t += Time.unscaledDeltaTime;
                    if (continueIndicator != null)
                        continueIndicator.SetActive(Mathf.FloorToInt(t / blinkInterval) % 2 == 0);
                    yield return null;
                }
            }

            running = null;
            var callback = onFinished;
            onFinished = null;
            Close();
            callback?.Invoke();
        }

        protected override void HandleClosing()
        {
            if (running != null) { StopCoroutine(running); running = null; }
        }
    }
}
