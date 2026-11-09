using System;
using System.Collections.Generic;
using UnityEngine;
using UniQuest.Contracts;

namespace UniQuest.UI.DebugTools
{
    // ─────────────────────────────────────────────────────────────────────────────
    //  DONNÉES FACTICES — pour tester l'UI AVANT que le gameplay d'Ilies soit prêt.
    //  Ces classes imitent ce que fera le vrai code. Quand le vrai code est branché,
    //  ce dossier Debug peut être supprimé (ou gardé dans une scène de test).
    // ─────────────────────────────────────────────────────────────────────────────

    public class FakeAttack : IAttackInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public int Power { get; set; }
        public int Accuracy { get; set; } = 95;
        public int CritChance { get; set; } = 10;
        public int ManaCost { get; set; }
        public bool IsMagic => ManaCost > 0;
        public int UnlockLevel { get; set; } = 1;
        public string Description { get; set; } = "";
    }

    public class FakeWeapon : IWeaponInfo
    {
        public string Name { get; set; }
        public int UpgradeLevel { get; set; }
        public int AttackBonus => 4 + UpgradeLevel * 3;
        public Sprite Icon { get; set; }
    }

    public class FakeItem : IItemInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public ItemCategory Category { get; set; }
        public int Quantity { get; set; }
        public bool UsableInFight => Category == ItemCategory.Heal || Category == ItemCategory.Snack || Category == ItemCategory.Boost;
        public bool UsableOutsideFight => UsableInFight;
        public bool NeedsTarget => UsableInFight;
        public int HealHP { get; set; }
        public int HealMP { get; set; }
    }

    public class FakeCombatant : ICombatant
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public int Level { get; private set; } = 1;
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int CurrentMP { get; private set; }
        public int MaxMP { get; private set; }
        public int CurrentXP { get; private set; }
        public int XPToNextLevel => 20 + Level * 15;
        public int BaseAttack { get; set; }
        public int Attack => BaseAttack + (Weapon != null ? Weapon.AttackBonus : 0);
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Accuracy { get; set; } = 95;
        public bool IsKO => CurrentHP <= 0;
        public Sprite BattleSprite { get; set; }
        public Sprite Portrait => BattleSprite;
        public List<IAttackInfo> AttackList { get; } = new List<IAttackInfo>();
        public IReadOnlyList<IAttackInfo> Attacks => AttackList;
        public FakeWeapon FakeWeapon { get; set; }
        public IWeaponInfo Weapon => FakeWeapon;

        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnManaChanged;
        public event Action<int, int> OnExperienceChanged;
        public event Action<int> OnLevelChanged;

        public FakeCombatant(int level, int hp, int mp)
        {
            Level = level;
            MaxHP = CurrentHP = hp;
            MaxMP = CurrentMP = mp;
        }

        public void ChangeHP(int delta)
        {
            CurrentHP = Mathf.Clamp(CurrentHP + delta, 0, MaxHP);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public void ChangeMP(int delta)
        {
            CurrentMP = Mathf.Clamp(CurrentMP + delta, 0, MaxMP);
            OnManaChanged?.Invoke(CurrentMP, MaxMP);
        }

        /// <summary>Renvoie true si le perso a gagné au moins un niveau.</summary>
        public bool GainXP(int amount)
        {
            bool leveled = false;
            CurrentXP += amount;
            while (CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                Level++;
                MaxHP += 8; BaseAttack += 2; Defense += 1; Speed += 1;
                leveled = true;
                OnLevelChanged?.Invoke(Level);
                OnHealthChanged?.Invoke(CurrentHP, MaxHP);
            }
            OnExperienceChanged?.Invoke(CurrentXP, XPToNextLevel);
            return leveled;
        }
    }
}
