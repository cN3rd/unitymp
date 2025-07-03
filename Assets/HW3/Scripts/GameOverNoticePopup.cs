using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class GameOverNoticePopup : BasePanel
    {
        [Header("Popup-specific")]
        [SerializeField] private GameManager manager;
        [SerializeField] private Button closeButton;
        
        private void OnEnable() => closeButton.onClick.AddListener(CloseButtonClicked);

        private void OnDisable() => closeButton.onClick.RemoveListener(CloseButtonClicked);

        // Note that this happens on the client
        private void CloseButtonClicked() => SceneManager.LoadScene("LobbyScene");

        public new void ShowPopup() => base.ShowPopup();
    }
}
