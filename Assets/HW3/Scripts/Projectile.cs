using Fusion;
using UnityEngine;

namespace HW3.Scripts
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private GameObject hitFxPrefab;

        [Networked] private TickTimer LifetimeTimer { get; set; }

        private const string PlayerTag = "Player";

        public override void Spawned()
        {
            base.Spawned();
            if (!Object.HasStateAuthority) return;
            
            LifetimeTimer = TickTimer.CreateFromSeconds(Runner, lifetime);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (!HasStateAuthority) return;

            transform.Translate(transform.forward * speed * Runner.DeltaTime);
            if (LifetimeTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"other: {other}");
            if (!other.CompareTag(PlayerTag)) return;

            var characterHealth = other.GetComponent<CharacterHealth>();
            var characterTransform = other.transform;
            var hitfx = Instantiate(hitFxPrefab, characterTransform.position, characterTransform.rotation);
            // Destroy(hitfx, 5f);
            
            if (HasStateAuthority && !characterHealth.Object.HasStateAuthority)
            {
                characterHealth.TakeDamageRPC(10);
                Runner.Despawn(Object);
            }
        }
    }
}
