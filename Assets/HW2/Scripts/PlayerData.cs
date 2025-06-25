using Fusion;

namespace HW2.Scripts
{
    public class PlayerData : NetworkBehaviour
    {
        [Networked]
        [OnChangedRender(nameof(UpdatePlayerNickname))]
        public NetworkString<_32> Nickname { get; set; }

        public override void Spawned() => Runner.MakeDontDestroyOnLoad(gameObject);

        private void UpdatePlayerNickname() =>
            PlayerNameContainer.Instance?.NotifyPlayerNameChangeRPC(Nickname);
    }
}
