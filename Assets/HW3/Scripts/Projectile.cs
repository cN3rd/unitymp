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
            if (!other.CompareTag(PlayerTag)) return;
            if (!HasStateAuthority) return;
            
            SpawnHitEffectRPC(other.transform.position, Quaternion.identity);
        
            var characterHealth = other.GetComponent<CharacterHealth>();
            if (characterHealth.Object.HasStateAuthority) return;
            
            characterHealth.TakeDamageRPC(10);
            LifetimeTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void SpawnHitEffectRPC(Vector3 position, Quaternion rotation)
        {
            var hitfx = Instantiate(hitFxPrefab, position, rotation);
            Destroy(hitfx, 5);
        }
    }
}
