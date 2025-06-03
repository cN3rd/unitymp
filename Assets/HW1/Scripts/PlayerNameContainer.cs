using System;
using System.Linq;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace HW1.Scripts
{
    public class PlayerNameContainer : NetworkBehaviour
    {
        public event Action<List<string>> OnPlayerNamesChanged;

        private void UpdateUI()
        {
            var list = PlayerNames.Select(x => x.Value.Value).ToList();
            OnPlayerNamesChanged?.Invoke(list);
        }
        
        [Networked] [Capacity(100)]
        private NetworkDictionary<PlayerRef, NetworkString<_64>> PlayerNames => default;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void NotifyPlayerJoinRPC(string playerName, RpcInfo info = default)
        {
            PlayerNames.Add(info.Source, playerName);
            UpdateUI();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void NotifyPlayerLeaveRPC(RpcInfo info = default)
        {
            PlayerNames.Remove(info.Source);
            UpdateUI();
        }

        public override void Spawned()
        {
            LobbyManager.Instance.SetPlayerNameContainer(this);
        }
    }
}
