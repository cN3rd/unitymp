using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace HW3.Scripts
{
    public class PlayerNameContainer : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public static PlayerNameContainer Instance;

        [Networked]
        [Capacity(20)]
        [OnChangedRender(nameof(OnPlayerDictionaryChanged))]
        private NetworkDictionary<PlayerRef, NetworkString<_32>> Players => default;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player != Runner.LocalPlayer) return;

            AddPlayerRPC("unknown");
            OnPlayerDictionaryChanged();
        }

        public void PlayerLeft(PlayerRef player) => RemovePlayerRPC(player);

        public event Action<List<string>> OnPlayerListChanged;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void NotifyPlayerNameChangeRPC(NetworkString<_32> newPlayerName, RpcInfo info = default)
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

        public override void Despawned(NetworkRunner runner, bool hasState) =>
            OnPlayerListChanged?.Invoke(new List<string>());
    }
}
