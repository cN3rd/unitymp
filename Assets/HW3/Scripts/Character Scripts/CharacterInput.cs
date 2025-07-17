using System;
using Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HW3.Scripts
{
    public class CharacterInput : NetworkBehaviour
    {    
        private InputSystemActions _inputActions;
        
        public event Action<NetworkObject> OnAttack;
        public event UnityAction OnMove;
        public Vector2 Direction { get; private set; }
        
        private void Start()
        {
            if (!HasInputAuthority)
                return;

            _inputActions = new InputSystemActions();
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Attack.performed += OnAttackPerformed;
            _inputActions.Enable();
        }


        private void OnAttackPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Attack!");
            OnAttack?.Invoke(Object);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("Movement Canceled");
            Direction = Vector2.zero;
        }

        private void OnMovePerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Movement Performed");
            Direction = obj.ReadValue<Vector2>();
            OnMove?.Invoke();
        }
    }
}
