using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HW3.Scripts {
    public class CharacterMovement : NetworkBehaviour
    {
        [SerializeField] private float speed = 5f;

        private InputSystemActions _inputActions;
        private Vector2 _direction;

        public event Action<CharacterMovement> OnAttack;

        void Start()
        {
            if (!HasInputAuthority)
                return;

            _inputActions = new InputSystemActions();
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Attack.performed += OnAttackPerformedd;
            _inputActions.Enable();
        }

        private void OnAttackPerformedd(InputAction.CallbackContext obj)
        {
            Debug.Log("Attack!");
            OnAttack?.Invoke(this);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("Movement Canceled");
            _direction = Vector2.zero;
        }

        private void OnMovePerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Movement Performed");
            _direction = obj.ReadValue<Vector2>();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority)
                return;

            var actualDirection = transform.forward * _direction.y + transform.right * _direction.x;
            var movement = actualDirection * (speed * Runner.DeltaTime);
            transform.Translate(movement);
        }
    }
}
