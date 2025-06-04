using TMPro;
using UnityEngine;

namespace HW1.Scripts
{
    public class FunUsernameSetter : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInputField;

        void Start() => usernameInputField.text = SillyId.GenerateGamertag();
    }
}
