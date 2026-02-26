using System;

namespace Sample
{
    /// <summary>
    /// HPを管理するドメインクラス。
    /// </summary>
    public class Hp
    {
        public int Current { get; private set; }
        public int Max { get; }
        public bool IsDead => Current <= 0;

        public Hp(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), "Max HP must be greater than 0.");
            Max = max;
            Current = max;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount must be non-negative.");
            Current = Math.Max(0, Current - amount);
        }

        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount must be non-negative.");
            Current = Math.Min(Max, Current + amount);
        }
    }
}
