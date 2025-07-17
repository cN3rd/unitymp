using Fusion;
using UnityEngine;
using HW3.Scripts;

namespace HW3.Scripts
{
    public class CharacterEvents : NetworkBehaviour
    {
        [SerializeField] private CharacterInput characterInput;
        [SerializeField] private CharacterVisualController characterVisualController;
        [SerializeField] private CharacterHealth characterHealth;

        public override void Spawned()
        {
            if (!HasInputAuthority)
                return;

            characterInput.OnMove += characterVisualController.PlayWalkAnimation;
            characterInput.OnAttack += characterVisualController.PlayAttackAnimation;
            characterHealth.OnTakingDamage += characterVisualController.PlayOnHitAnimation;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority)
                return;

            characterInput.OnMove -= characterVisualController.PlayWalkAnimation;
            characterHealth.OnTakingDamage -= characterVisualController.PlayOnHitAnimation;
            characterInput.OnAttack -= characterVisualController.PlayAttackAnimation;
        }
    }
}
