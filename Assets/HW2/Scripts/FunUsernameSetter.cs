using TMPro;
using UnityEngine;

namespace HW2.Scripts
{
    public class FunUsernameSetter : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInputField;

        void Start() => usernameInputField.text = SillyId.GenerateGamertag();
    }
}
