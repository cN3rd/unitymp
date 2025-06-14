using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW1.Scripts
{
    public class ErrorPopup : PopupBase
    {
        [Header("Popup-specific")]
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button closeButton;

        private void OnEnable() => closeButton.onClick.AddListener(CloseButtonClicked);
        private void OnDisable() => closeButton.onClick.RemoveListener(CloseButtonClicked);

        private void CloseButtonClicked() => HidePopup();

        public void ShowError(string error)
        {
            ShowPopup();
            errorText.text = error;
        }
    }
}
