using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace HW2.Scripts
{
    public class PlayerNameContainer : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Networked, Capacity(20), OnChangedRender(nameof(OnPlayerDictionaryChanged))]
        private NetworkDictionary<PlayerRef, NetworkString<_32>> Players => default;

        public static PlayerNameContainer Instance;
        
        private void Start()
        {
            if (Instance == null) Instance = this;
        }

        public event Action<List<string>> OnPlayerListChanged;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void NotifyPlayerNameChangeRPC(NetworkString<_32> newPlayerName,RpcInfo info = default)
        {
            Debug.Log($"Got change name request: {info.Source}, {newPlayerName}");
            if (!Object.HasStateAuthority) return;

            Players.Set(info.Source, newPlayerName);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void AddPlayerRPC(NetworkString<_32> playerName, RpcInfo info = default)
        {
            if (!Object.HasStateAuthority) return;

            Players.Set(info.Source, playerName);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RemovePlayerRPC(PlayerRef playerToRemove)
        {
            if (!Object.HasStateAuthority) return;

            Players.Remove(playerToRemove);
        }

        private void OnPlayerDictionaryChanged()
        {
            var playerList = Players.Select(kvp => kvp.Value.ToString()).ToList();
            OnPlayerListChanged?.Invoke(playerList);
        }

        public override void Spawned()
        {
            LobbyManager.Instance.SetPlayerNameContainer(this);
            PlayerJoined(Runner.LocalPlayer);
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player != Runner.LocalPlayer) return;
            AddPlayerRPC("unknown");
            OnPlayerDictionaryChanged();
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void Rpc_SendPrivateMessage(PlayerRef targetPlayer, string message, RpcInfo info = default)
        {
            if (Runner.LocalPlayer == targetPlayer)
            {
                PrivateMessageUI.Instance?.ShowMessage(message);
            }
        }

        public void PlayerLeft(PlayerRef player) => RemovePlayerRPC(player);

        public override void Despawned(NetworkRunner runner, bool hasState) => OnPlayerListChanged?.Invoke(new List<string>());
    }
}
