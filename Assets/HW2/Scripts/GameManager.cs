using Fusion;
using HW2.Scripts.Testing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW2.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private GameOverNoticePopup gameOverNoticePopup;
        [SerializeField] private CharacterSelectionPanel characterSelectionPanel;
        [SerializeField] private TopPanelUI topPanel;

        private void Start()
        {
            characterSelectionPanel.OnCharacterSelected += topPanel.ShowTopPanel;
            topPanel.OnCloseSessionRequested += CloseSession;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Runner.SessionInfo.IsOpen = false;
            }
            base.Spawned();
            Runner.MakeDontDestroyOnLoad(this.gameObject);
        }

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
    }
}
