using UnityEngine;

namespace Battle.Units
{
    [System.Serializable]
    public class UnitStats
    {
        public int MaxHP;
        public int CurrentHP;
        public int Attack;
        public int Defense;
        public int Agility;

        public UnitStats(int maxHp, int atk, int def, int agi)
        {
            MaxHP = maxHp;
            CurrentHP = maxHp;
            Attack = atk;
            Defense = def;
            Agility = agi;
        }

        public void TakeDamage(int rawDamage)
        {
            int damage = Mathf.Max(1, rawDamage - Defense);
            CurrentHP -= damage;
            CurrentHP = Mathf.Max(0, CurrentHP);
            Debug.Log($"[UnitStats] {damage} 데미지 입음 (남은 HP: {CurrentHP})");
        }

        public bool IsDead => CurrentHP <= 0;

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }
    }
}
