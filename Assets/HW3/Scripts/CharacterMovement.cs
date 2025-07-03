using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HW3.Scripts
{
    public class CharacterMovement : NetworkBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private CharacterInput input;
        
        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority)
                return;

            Vector3 actualDirection = transform.forward * input.Direction.y + transform.right * input.Direction.x;
            Vector3 movement = actualDirection * (speed * Runner.DeltaTime);
            transform.Translate(movement);
        }
    }
}
