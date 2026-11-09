using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Le sprite d'un combattant à l'écran, avec de petites animations faites en code
    /// (pas besoin d'Animator) : élan quand il attaque, tremblement + flash quand il est touché,
    /// disparition quand il est KO.
    ///
    /// L'échelle est un ENTIER (×3 pour les persos 64×64) : sinon le pixel art devient flou.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BattlerView : MonoBehaviour
    {
        [SerializeField] private int pixelScale = 3;
        [Tooltip("Échelle utilisée pour un ennemi évolué ou un boss")]
        [SerializeField] private int elitePixelScale = 4;
        [Tooltip("+1 si le combattant regarde vers la droite (joueur), -1 vers la gauche (ennemi)")]
        [SerializeField] private int facing = 1;
        [SerializeField] private float lungeDistance = 24f;
        [SerializeField] private float shakeDistance = 6f;

        private Image image;
        private RectTransform rect;
        private Vector2 basePosition;
        private bool initialized;

        private void Init()
        {
            if (initialized) return;
            image = GetComponent<Image>();
            rect = (RectTransform)transform;
            basePosition = rect.anchoredPosition;
            image.preserveAspect = true;
            initialized = true;
        }

        public void Bind(ICombatant combatant, bool elite = false)
        {
            Init();
            StopAllCoroutines();
            rect.anchoredPosition = basePosition;
            image.color = Color.white;

            image.sprite = combatant != null ? combatant.BattleSprite : null;
            image.enabled = image.sprite != null;
            if (image.sprite == null) return;

            int scale = elite ? elitePixelScale : pixelScale;
            Vector2 size = image.sprite.rect.size;
            rect.sizeDelta = size * scale;
        }

        public IEnumerator Lunge()
        {
            Init();
            Vector2 forward = new Vector2(lungeDistance * facing, 0f);
            yield return Move(basePosition, basePosition + forward, 0.08f);
            yield return Move(basePosition + forward, basePosition, 0.12f);
        }

        public IEnumerator Hit()
        {
            Init();
            for (int i = 0; i < 6; i++)
            {
                image.color = i % 2 == 0 ? new Color(1f, 1f, 1f, 0.3f) : Color.white;
                float dir = i % 2 == 0 ? 1f : -1f;
                rect.anchoredPosition = basePosition + new Vector2(shakeDistance * dir, 0f);
                yield return new WaitForSecondsRealtime(0.05f);
            }
            image.color = Color.white;
            rect.anchoredPosition = basePosition;
        }

        public IEnumerator Miss()
        {
            Init();
            // Petit pas de côté : l'attaque passe à côté
            Vector2 dodge = basePosition + new Vector2(-lungeDistance * facing * 0.6f, 0f);
            yield return Move(basePosition, dodge, 0.08f);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return Move(dodge, basePosition, 0.12f);
        }

        public IEnumerator Faint()
        {
            Init();
            Vector2 down = basePosition + new Vector2(0f, -40f);
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.4f;
                rect.anchoredPosition = Vector2.Lerp(basePosition, down, k);
                image.color = new Color(1f, 1f, 1f, 1f - k);
                yield return null;
            }
            image.enabled = false;
            rect.anchoredPosition = basePosition;
        }

        private IEnumerator Move(Vector2 from, Vector2 to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                Vector2 p = Vector2.Lerp(from, to, t / duration);
                rect.anchoredPosition = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
                yield return null;
            }
            rect.anchoredPosition = to;
        }
    }
}
