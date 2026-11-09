using UnityEngine;
using UnityEngine.EventSystems;

namespace UniQuest.UI
{
    /// <summary>
    /// Le petit triangle doré des maquettes : il se place à gauche du bouton sélectionné
    /// et oscille légèrement. Un seul curseur suffit pour tout le Canvas.
    ///
    /// Montage : une Image (le triangle, en pixel art) enfant du Canvas, tout en bas de la
    /// hiérarchie pour être dessinée par-dessus le reste. Pivot à droite au milieu (1, 0.5).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SelectionCursor : MonoBehaviour
    {
        [Tooltip("Décalage par rapport au bord gauche du bouton, en pixels de référence")]
        [SerializeField] private Vector2 offset = new Vector2(-8f, 0f);
        [SerializeField] private float bobAmplitude = 3f;
        [SerializeField] private float bobSpeed = 6f;
        [Tooltip("Arrondit la position au pixel près (indispensable pour le pixel art)")]
        [SerializeField] private bool snapToPixel = true;

        private RectTransform rect;
        private Canvas canvas;
        private CanvasGroupHider visual;
        private readonly Vector3[] corners = new Vector3[4];

        // Petit utilitaire pour masquer sans désactiver le GameObject (sinon LateUpdate ne tourne plus)
        private struct CanvasGroupHider
        {
            public CanvasGroup group;
            public void Show(bool v) { if (group != null) group.alpha = v ? 1f : 0f; }
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            var g = GetComponent<CanvasGroup>();
            if (g == null) g = gameObject.AddComponent<CanvasGroup>();
            g.blocksRaycasts = false;
            g.interactable = false;
            visual.group = g;
        }

        private void LateUpdate()
        {
            var es = EventSystem.current;
            var selected = es != null ? es.currentSelectedGameObject : null;

            // Le curseur ne s'affiche que sur les boutons qui portent un MenuButton
            // (pas sur les emplacements d'inventaire, qui ont leur propre cadre)
            if (selected == null || !selected.activeInHierarchy || selected.GetComponent<MenuButton>() == null
                || selected.GetComponent<NoCursor>() != null)
            {
                visual.Show(false);
                return;
            }

            var target = selected.transform as RectTransform;
            if (target == null) { visual.Show(false); return; }

            target.GetWorldCorners(corners);
            Vector3 leftMiddle = (corners[0] + corners[1]) * 0.5f;

            float scale = canvas != null ? canvas.scaleFactor : 1f;
            float bob = Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmplitude;
            Vector3 pos = leftMiddle + new Vector3((offset.x + bob) * scale, offset.y * scale, 0f);

            if (snapToPixel)
                pos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), pos.z);

            rect.position = pos;
            visual.Show(true);
        }
    }

    /// <summary>Marqueur : ajoute-le sur un bouton qui ne doit pas afficher le curseur.</summary>
    public class NoCursor : MonoBehaviour { }
}
