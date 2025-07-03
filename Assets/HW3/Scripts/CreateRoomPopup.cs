using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class CreateRoomPopup : BasePanel
    {
        [Header("Popup-specific")]
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_Dropdown numberPlayersDropdown;
        [SerializeField] private Button createRoomButton;

        private void OnEnable() => createRoomButton.onClick.AddListener(CreateRoomButtonClicked);
        private void OnDisable() => createRoomButton.onClick.RemoveListener(CreateRoomButtonClicked);

        public event Action<CreateRoomDetails> OnCreateRoom;

        public new void ShowPopup() => base.ShowPopup();

        public void CreateRoomButtonClicked()
        {
            int numPlayers = int.Parse(numberPlayersDropdown.options[numberPlayersDropdown.value].text);
            CreateRoomDetails record = new(roomNameInput.text, numPlayers);
            HidePopup();
            OnCreateRoom?.Invoke(record);
        }
    }

    public record CreateRoomDetails(string name, int numPlayers);
}
