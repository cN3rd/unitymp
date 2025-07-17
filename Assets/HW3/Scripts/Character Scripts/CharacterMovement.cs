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

            Vector3 inputDir = new Vector3(input.Direction.x, 0, input.Direction.y);

            if (inputDir.sqrMagnitude > 0.01f)
            {
                // Move
                Vector3 move = inputDir.normalized * (speed * Runner.DeltaTime);
                transform.Translate(move, Space.World);

                // Rotate
                Quaternion targetRotation = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Runner.DeltaTime);
            }
        }
    }
}
