using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW1.Scripts
{
    public class CreateRoomPopup : PopupBase
    {
        [Header("Popup-specific")]
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_Dropdown numberPlayersDropdown;
        [SerializeField] private Button createRoomButton;

        public event Action<CreateRoomDetails> OnCreateRoom ;
        
        private void OnEnable() => createRoomButton.onClick.AddListener(CreateRoomButtonClicked);
        private void OnDisable() => createRoomButton.onClick.RemoveListener(CreateRoomButtonClicked);

        public new void ShowPopup() => base.ShowPopup();

        public void CreateRoomButtonClicked()
        {
            var numPlayers = int.Parse(numberPlayersDropdown.options[numberPlayersDropdown.value].text);
            var record = new CreateRoomDetails(roomNameInput.text, numPlayers);
            HidePopup();
            OnCreateRoom?.Invoke(record);
        }
    }

    public record CreateRoomDetails(string name, int numPlayers);
}
