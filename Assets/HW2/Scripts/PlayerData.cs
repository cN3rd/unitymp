using Fusion;
using TMPro;
using UnityEngine;

namespace HW2.Scripts
{
    public class PlayerData : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(UpdatePlayerNickname))] public NetworkString<_32> Nickname { get; set; }

        [SerializeField] private GameObject chatUIPrefab;
        private ChatManager localChat;

        public override void Spawned()
        {
            Runner.MakeDontDestroyOnLoad(gameObject);

            if (Object.HasInputAuthority)
            {
                GameObject chatUI = Instantiate(chatUIPrefab);
                localChat = chatUI.GetComponent<ChatManager>();
                localChat.SetOwner(this); //  Link to PlayerData for sending messages
            }
        }

        private void UpdatePlayerNickname() => PlayerNameContainer.Instance?.NotifyPlayerNameChangeRPC(Nickname);

        // This is called by ChatManager when Enter is pressed and a message is typed
        public void SendChatMessage(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Rpc_DeliverChatMessage(msg);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void Rpc_DeliverChatMessage(string msg, RpcInfo info = default)
        {
            foreach (ChatManager chat in FindObjectsOfType<ChatManager>())
            {
                chat.ReceiveMessageLocally($"Player {info.Source.PlayerId}: {msg}");
            }
        }
    }
}
