using Fusion;
using UnityEngine;

namespace HW3.Scripts
{
    public class GameManager : NetworkBehaviour, IPlayerLeft
    {
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private GameOverNoticePopup gameOverNoticePopup;
        [SerializeField] private CharacterSelectionPanel characterSelectionPanel;
        [SerializeField] private DirectMessageSendPanel dmSendPanel;
        [SerializeField] private DirectMessageView dmView;
        [SerializeField] private TopPanelUI topPanel;

        private void Start()
        {
            characterSelectionPanel.OnCharacterSelected += topPanel.ShowTopPanel;
            topPanel.OnCloseSessionRequested += CloseSession;
            topPanel.OnDirectMessageRequested += dmSendPanel.ShowPanel;
            dmSendPanel.OnSendDirectMessage += OnSendDirectMessage;
        }

        public override void Spawned()
        {
            base.Spawned();

            if (HasStateAuthority)
                Runner.SessionInfo.IsOpen = false;

            Runner.MakeDontDestroyOnLoad(gameObject);
            RPC_RefreshPlayers();
        }

        public void PlayerLeft(PlayerRef player) => RPC_RefreshPlayers();

        private void OnSendDirectMessage(PlayerRef receiverRef, string sender, string message) =>
            RPC_DirectMessage(receiverRef, sender, message);

        private void CloseSession()
        {
            if (Runner.IsSharedModeMasterClient || Object.HasStateAuthority)
            {
                RPC_CloseSession();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CloseSession()
        {
            async Awaitable DoAsync()
            {
                gameOverNoticePopup.ShowPopup();
                await Awaitable.WaitForSecondsAsync(1);

                Runner.Despawn(Runner.GetPlayerObject(Runner.LocalPlayer));
                await Runner.Shutdown(shutdownReason: ShutdownReason.GameClosed);
            }

            _ = DoAsync();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_DirectMessage([RpcTarget] PlayerRef target, string sender, string message)
        {
            Debug.Log($"New DM: {sender}: {message}");
            dmView.OnNewMessage(sender, message);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_RefreshPlayers() => dmSendPanel.UpdateData(Runner);
    }
}
