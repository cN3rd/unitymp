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
        [SerializeField] Vector3 heightOffset;

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

        const string projectileTag = "Projectile";
        private const float hitRadius = 1f;
        private Collider[] hitColliders = new Collider[10];

        [Rpc]
        public void TakeDamageRPC(int damage)
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position + heightOffset, hitRadius, hitColliders);
            bool validBullet = false;
            
            for (int i = 0; i < hits; i++)
            {
                if (!hitColliders[i]) continue;
                Debug.Log(hitColliders[i]);
                if (!hitColliders[i].CompareTag(projectileTag)) continue;
                validBullet = true;
            }

            if (!validBullet) return;
            TakeDamage(damage);
        }

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

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position + heightOffset, hitRadius);
        }

        private void OnValidate()
        {
            heightOffset = GetComponent<CapsuleCollider>().center;
        }

#endif
    }
}
