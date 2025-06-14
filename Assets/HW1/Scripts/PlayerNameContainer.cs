using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace HW1.Scripts
{
    public class PlayerNameContainer : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Networked, Capacity(20), OnChangedRender(nameof(OnPlayerNameDictionaryChanged))]
        private NetworkDictionary<PlayerRef, NetworkString<_32>> PlayerNames => default;

        public event Action<List<string>> OnPlayerNamesChanged;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void AddPlayerRPC(string playerName, RpcInfo info = default)
        {
            if (!Object.HasStateAuthority) return;

            PlayerNames.Set(info.Source, playerName);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RemovePlayerRPC(PlayerRef playerToRemove)
        {
            if (!Object.HasStateAuthority) return;

            PlayerNames.Remove(playerToRemove);
        }

        private void OnPlayerNameDictionaryChanged()
        {
            var playerList = PlayerNames.Select(kvp => kvp.Value.ToString()).ToList();
            OnPlayerNamesChanged?.Invoke(playerList);
        }

        public override void Spawned()
        {
            LobbyManager.Instance.SetPlayerNameContainer(this);
            
            string playerName = LobbyManager.Instance.GetCurrentPlayerName();
            if (!string.IsNullOrEmpty(playerName))
            {
                AddPlayerRPC(playerName);
            }
            
            OnPlayerNameDictionaryChanged();
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player != Runner.LocalPlayer) return;

            string playerName = LobbyManager.Instance.GetCurrentPlayerName();
            if (string.IsNullOrEmpty(playerName)) return;

            AddPlayerRPC(playerName);
        }

        public void PlayerLeft(PlayerRef player) => RemovePlayerRPC(player);

        public override void Despawned(NetworkRunner runner, bool hasState) => OnPlayerNamesChanged?.Invoke(new List<string>());
    }
}
