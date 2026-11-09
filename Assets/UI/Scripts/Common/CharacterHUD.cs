using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniQuest.Contracts;

namespace UniQuest.UI
{

    /// Panneau d'infos d'un personnage
    
    public class CharacterHUD : MonoBehaviour
    {
        [Header("Textes")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [Tooltip("Optionnel")]
        [SerializeField] private TMP_Text typeText;

        [Header("Barres (optionnelles sauf PV)")]
        [SerializeField] private StatBar hpBar;
        [SerializeField] private StatBar mpBar;
        [SerializeField] private StatBar xpBar;

        [Header("Visuel (optionnel)")]
        [Tooltip("Image qui affiche le sprite de combat ou le portrait")]
        [SerializeField] private Image spriteImage;
        [SerializeField] private bool usePortrait = false;
        [Tooltip("Étiquette \"ÉVOLUÉE\" / \"BOSS\", activée par SetElite")]
        [SerializeField] private GameObject eliteBadge;

        private ICombatant character;

        // Mode "différé" (combat) : les PV changent dans le gameplay AVANT que le message
        // "Trochu utilise..." soit affiché. On garde les nouvelles valeurs de côté
        // et FightUI les applique (Flush) au moment où l'attaque touche à l'écran.
        private bool deferUpdates;
        private bool hasPendingHp, hasPendingMp, hasPendingXp;
        private int pendingHp, pendingHpMax, pendingMp, pendingMpMax, pendingXp, pendingXpMax;

        public ICombatant Character => character;

        /// Branche le HUD sur un personnage. Rappelle-la quand un perso KO est remplacé :
        public void Bind(ICombatant newCharacter)
        {
            Unbind();
            character = newCharacter;
            hasPendingHp = hasPendingMp = hasPendingXp = false;
            gameObject.SetActive(character != null);
            if (character == null) return;

            character.OnHealthChanged += HandleHealthChanged;
            character.OnManaChanged += HandleManaChanged;
            character.OnExperienceChanged += HandleExperienceChanged;
            character.OnLevelChanged += HandleLevelChanged;

            Refresh();
        }

        /// Redessine tout sans animation (utile à l'ouverture d'un menu).
        public void Refresh()
        {
            if (character == null) return;

            if (nameText != null) nameText.text = character.Name;
            if (typeText != null) typeText.text = $"Type : {character.TypeName}";
            HandleLevelChanged(character.Level);

            if (hpBar != null) hpBar.SetValue(character.CurrentHP, character.MaxHP, instant: true);
            if (mpBar != null) mpBar.SetValue(character.CurrentMP, character.MaxMP, instant: true);
            if (xpBar != null) xpBar.SetValue(character.CurrentXP, character.XPToNextLevel, instant: true);

            if (spriteImage != null)
            {
                Sprite s = usePortrait && character.Portrait != null ? character.Portrait : character.BattleSprite;
                spriteImage.sprite = s;
                spriteImage.enabled = s != null;
                spriteImage.preserveAspect = true;
            }
        }

        public void SetElite(bool elite)
        {
            if (eliteBadge != null) eliteBadge.SetActive(elite);
        }

        private void Unbind()
        {
            if (character == null) return;
            // Toujours se désabonner, sinon le personnage garde une référence vers un HUD détruit
            character.OnHealthChanged -= HandleHealthChanged;
            character.OnManaChanged -= HandleManaChanged;
            character.OnExperienceChanged -= HandleExperienceChanged;
            character.OnLevelChanged -= HandleLevelChanged;
            character = null;
        }

        private void OnDestroy() => Unbind();

        /// Active/désactive le mode différé. En le désactivant, tout est appliqué.
        public void SetDeferred(bool value)
        {
            deferUpdates = value;
            if (!value) Flush();
        }

        /// Applique (avec animation) les valeurs mises de côté en mode différé.
        public void Flush()
        {
            // "if (x != null)" et pas "x?." : l'opérateur ?. ne détecte pas un objet Unity détruit
            if (hasPendingHp && hpBar != null) hpBar.SetValue(pendingHp, pendingHpMax);
            if (hasPendingMp && mpBar != null) mpBar.SetValue(pendingMp, pendingMpMax);
            if (hasPendingXp && xpBar != null) xpBar.SetValue(pendingXp, pendingXpMax);
            hasPendingHp = hasPendingMp = hasPendingXp = false;
        }

        private void HandleHealthChanged(int current, int max)
        {
            pendingHp = current; pendingHpMax = max; hasPendingHp = true;
            if (!deferUpdates) Flush();
        }

        private void HandleManaChanged(int current, int max)
        {
            pendingMp = current; pendingMpMax = max; hasPendingMp = true;
            if (!deferUpdates) Flush();
        }

        private void HandleExperienceChanged(int current, int toNextLevel)
        {
            pendingXp = current; pendingXpMax = toNextLevel; hasPendingXp = true;
            if (!deferUpdates) Flush();
        }

        private void HandleLevelChanged(int level)
        {
            if (levelText != null) levelText.text = $"Niv. {level}";
        }
    }
}
