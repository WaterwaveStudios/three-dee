using UnityEngine;

namespace ThreeDee.Core
{
    public class PlayerHealth : MonoBehaviour
    {
        public int MaxHp { get; private set; } = 10;
        public int CurrentHp { get; private set; }

        public event System.Action OnDeath;
        public event System.Action<int, int> OnHealthChanged; // current, max

        private bool _dead;

        private void Awake()
        {
            CurrentHp = MaxHp;
        }

        public void TakeDamage(int amount)
        {
            if (_dead) return;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            if (CurrentHp <= 0)
            {
                _dead = true;
                OnDeath?.Invoke();
            }
        }
    }
}
