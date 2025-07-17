using Fusion;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HW3.Scripts
{
    public class CharacterHealth : NetworkBehaviour
    {
        public event UnityAction OnTakingDamage;
        [SerializeField] private int maxHealth = 100;

        [Networked]
        [OnChangedRender(nameof(OnHealthValueChanged))]
        public int CurrentHealth { get; set; } = 1;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        public override void Spawned()
        {
            if (HasStateAuthority)
                CurrentHealth = maxHealth;
        }

        //[ContextMenu()]
        [Rpc]
        public void TakeDamageRPC(int damage) => TakeDamage(damage);

        private void TakeDamage(int damage)
        {
            if (HasStateAuthority)
            {
                CurrentHealth -= damage;
                OnTakingDamage?.Invoke();
            }

            if (CurrentHealth <= 0)
                OnDeath?.Invoke();
        }

        private void OnHealthValueChanged()
        {
            float fraction = CurrentHealth / (float)maxHealth;
            OnHealthChanged?.Invoke(fraction);
        }
    }
}
