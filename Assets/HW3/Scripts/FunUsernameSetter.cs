using TMPro;
using UnityEngine;

namespace HW3.Scripts
{
    public class FunUsernameSetter : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInputField;

        private void Start() => usernameInputField.text = SillyId.GenerateGamertag();
    }
}
