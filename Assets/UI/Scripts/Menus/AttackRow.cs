using UnityEngine;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{
    /// <summary>
    /// Ligne d'attaque en lecture seule dans le menu équipe :
    /// "Séisme      Terre · 8 PM", ou "???      Débloquée au niv. 8" si pas encore apprise.
    /// </summary>
    public class AttackRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private UITheme theme;

        public void Setup(IAttackInfo attack, int ownerLevel)
        {
            bool unlocked = attack.UnlockLevel <= ownerLevel;
            if (unlocked)
            {
                nameText.text = attack.Name;
                infoText.text = attack.ManaCost > 0 ? $"{attack.TypeName} · {attack.ManaCost} PM" : attack.TypeName;
            }
            else
            {
                nameText.text = "???";
                infoText.text = $"Débloquée au niv. {attack.UnlockLevel}";
            }

            Color muted = theme != null ? theme.textMuted : (Color)new Color32(0xC4, 0xB6, 0xA4, 0xFF);
            Color disabled = theme != null ? theme.textDisabled : (Color)new Color32(0x8F, 0x84, 0x8A, 0xFF);
            Color text = theme != null ? theme.text : (Color)new Color32(0xEC, 0xE0, 0xCF, 0xFF);
            nameText.color = unlocked ? text : disabled;
            infoText.color = unlocked ? muted : disabled;
        }
    }
}
