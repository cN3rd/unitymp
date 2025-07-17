using Fusion;
using UnityEngine;
using HW3.Scripts;

namespace HW3.Scripts
{
    public class CharacterEvents : NetworkBehaviour
    {
        [SerializeField] CharacterInput characterInput;
        [SerializeField] CharacterVisualController characterVisualController;
        [SerializeField] CharacterHealth characterHealth;

        public override void Spawned()
        {
            if (!HasInputAuthority)
                return;

            characterInput.OnMove += characterVisualController.PlayWalkAnimation;
            characterHealth.OnTakingDamage += characterVisualController.PlayOnHitAnimation;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority)
                return;

            characterInput.OnMove -= characterVisualController.PlayWalkAnimation;
            characterHealth.OnTakingDamage -= characterVisualController.PlayOnHitAnimation;
        }
    }
}
