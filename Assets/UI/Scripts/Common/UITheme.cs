using UnityEngine;

namespace UniQuest.UI
{
    /// <summary>
    /// Les couleurs de l'UI, issues du moodboard, en un seul endroit.
    /// Création : clic droit dans Project > Create > UniQuest > UI Theme.
    /// Changer une couleur ici la change partout (barres, curseur, textes désactivés).
    /// </summary>
    [CreateAssetMenu(menuName = "UniQuest/UI Theme", fileName = "UITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("Palette du moodboard")]
        public Color violet = new Color32(0x57, 0x08, 0x84, 0xFF);   // cadres
        public Color blue = new Color32(0x14, 0x3E, 0x91, 0xFF);
        public Color blood = new Color32(0x7D, 0x20, 0x21, 0xFF);    // liserés, ennemis évolués

        [Header("Interface")]
        public Color accent = new Color32(0xE8, 0xB5, 0x4A, 0xFF);   // curseur doré
        public Color text = new Color32(0xEC, 0xE0, 0xCF, 0xFF);     // couleur os
        public Color textMuted = new Color32(0xC4, 0xB6, 0xA4, 0xFF);
        public Color textDisabled = new Color32(0x8F, 0x84, 0x8A, 0xFF);

        [Header("Barres")]
        public Color hp = new Color32(0xC8, 0x42, 0x3F, 0xFF);
        public Color hpLow = new Color32(0xE8, 0x87, 0x3A, 0xFF);
        public Color mp = new Color32(0x4A, 0x74, 0xD6, 0xFF);
        public Color xp = new Color32(0xC9, 0xB8, 0xA6, 0xFF);

        [Header("Messages de combat")]
        public Color critical = new Color32(0xE8, 0xB5, 0x4A, 0xFF);
        public Color heal = new Color32(0x7F, 0xC8, 0x7A, 0xFF);
    }
}
