using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HW2.Scripts
{
    public class DirectMessageSendPanel : BasePanel
    {
        [SerializeField] private TMP_Dropdown participantsDropdown;
        [SerializeField] private TMP_InputField messageInput;
        [SerializeField] private Button submitButton;

        private Dictionary<string, PlayerRef> _possibleReceivers = new();
        private string _sender;

        public Action<PlayerRef, string, string> OnSendDirectMessage;

        private void Awake()
        {
            messageInput.characterLimit = 128;
            submitButton.onClick.AddListener(Submit);
        }

        // not the most efficient way to do this but it's okay for now
        public void UpdateData(NetworkRunner runner)
        {
            _possibleReceivers = new Dictionary<string, PlayerRef>();

            foreach (var player in runner.ActivePlayers)
            {
                if (player == runner.LocalPlayer) continue;

                var playerObject = runner.GetPlayerObject(player);
                if (playerObject == null) continue;

                var playerData = playerObject.GetComponent<PlayerData>();
                if (playerData == null) continue;

                _possibleReceivers[playerData.Nickname.ToString()] = player;
            }

            participantsDropdown.ClearOptions();
            participantsDropdown.AddOptions(_possibleReceivers.Keys.ToList());

            _sender = runner.GetPlayerObject(runner.LocalPlayer).GetComponent<PlayerData>().Nickname
                .ToString();
        }

        public void ShowPanel() => base.ShowPopup();

        public void Submit()
        {
            var receiverString = participantsDropdown.options[participantsDropdown.value].text;
            var receiverRef = _possibleReceivers[receiverString];

            OnSendDirectMessage?.Invoke(receiverRef, _sender, messageInput.text);
        }
    }
}
