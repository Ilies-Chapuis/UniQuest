using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniQuest.UI
{
    /// <summary>
    /// Barre générique (PV, PM, XP). Elle ne connaît rien au jeu :
    /// on lui donne une valeur et un max, elle s'affiche.
    ///
    /// Montage : une Image de fond, avec en enfant une Image de remplissage
    /// en Image Type = Filled, Fill Method = Horizontal, glissée dans "fill".
    /// Pour l'effet "les PV descendent puis la traînée rattrape", ajoute une 2e Image
    /// Filled, plus claire, SOUS la barre principale, et glisse-la dans "trail".
    /// </summary>
    public class StatBar : MonoBehaviour
    {
        public enum BarKind { Health, Mana, Experience, Custom }

        [Header("Références")]
        [SerializeField] private Image fill;
        [Tooltip("Optionnel : traînée qui rattrape la barre en retard (effet de dégâts)")]
        [SerializeField] private Image trail;
        [Tooltip("Optionnel : affiche \"72/90\"")]
        [SerializeField] private TMP_Text valueText;

        [Header("Apparence")]
        [SerializeField] private BarKind kind = BarKind.Health;
        [Tooltip("Optionnel : si vide, les couleurs ci-dessous sont utilisées")]
        [SerializeField] private UITheme theme;
        [SerializeField] private Color customColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.3f;

        [Header("Animation")]
        [Tooltip("Fraction de barre parcourue par seconde")]
        [SerializeField] private float animationSpeed = 1.5f;
        [Tooltip("Délai avant que la traînée rattrape la barre")]
        [SerializeField] private float trailDelay = 0.4f;

        private float targetFill = 1f;
        private float trailTimer;

        public bool IsAnimating => fill != null &&
            (!Mathf.Approximately(fill.fillAmount, targetFill) ||
             (trail != null && !Mathf.Approximately(trail.fillAmount, targetFill)));

        /// <summary>
        /// Met à jour la barre. instant = true pour sauter l'animation
        /// (ouverture d'un menu, début de combat, changement de perso...).
        /// </summary>
        public void SetValue(int current, int max, bool instant = false)
        {
            if (fill == null) return;

            float previous = targetFill;
            targetFill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            if (instant)
            {
                fill.fillAmount = targetFill;
                if (trail != null) trail.fillAmount = targetFill;
            }
            else if (targetFill < previous)
            {
                trailTimer = trailDelay;   // dégâts : la traînée attend un peu avant de descendre
            }

            if (valueText != null)
                valueText.text = $"{Mathf.Max(0, current)}/{max}";

            fill.color = ResolveColor();
        }

        private Color ResolveColor()
        {
            bool low = targetFill <= lowThreshold;
            switch (kind)
            {
                case BarKind.Health:
                    if (theme != null) return low ? theme.hpLow : theme.hp;
                    return low ? new Color32(0xE8, 0x87, 0x3A, 0xFF) : new Color32(0xC8, 0x42, 0x3F, 0xFF);
                case BarKind.Mana:
                    return theme != null ? theme.mp : (Color)new Color32(0x4A, 0x74, 0xD6, 0xFF);
                case BarKind.Experience:
                    return theme != null ? theme.xp : (Color)new Color32(0xC9, 0xB8, 0xA6, 0xFF);
                default:
                    return customColor;
            }
        }

        private void Update()
        {
            if (fill == null) return;
            // unscaledDeltaTime : la barre s'anime même si le jeu est en pause (Time.timeScale = 0)
            float step = animationSpeed * Time.unscaledDeltaTime;

            if (trail == null)
            {
                fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, targetFill, step);
                return;
            }

            // Barre principale : tombe tout de suite (dégâts), monte doucement (soin)
            if (fill.fillAmount > targetFill) fill.fillAmount = targetFill;
            else fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, targetFill, step);

            // Traînée : attend le délai puis descend (dégâts), saute directement (soin)
            if (trail.fillAmount < targetFill)
            {
                trail.fillAmount = targetFill;
            }
            else if (trail.fillAmount > targetFill)
            {
                if (trailTimer > 0f) trailTimer -= Time.unscaledDeltaTime;
                else trail.fillAmount = Mathf.MoveTowards(trail.fillAmount, targetFill, step);
            }
        }
    }
}
