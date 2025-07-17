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
        
        public event Action OnAttack; // 
        public event UnityAction<float> OnMove;
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
            OnAttack?.Invoke();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("Movement Canceled");
            Direction = Vector2.zero;
            OnMove?.Invoke(Direction.magnitude); //Direction.magnitude - Use this for animation blending
        }

        private void OnMovePerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Movement Performed");
            Direction = obj.ReadValue<Vector2>();
            Debug.Log($"Movement Performed with speed: {Direction.magnitude}");
            OnMove?.Invoke(Direction.magnitude); //Direction.magnitude - Use this for animation blending
        }
    }
}
