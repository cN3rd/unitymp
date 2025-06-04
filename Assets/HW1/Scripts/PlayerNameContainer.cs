using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace HW1.Scripts
{
    public class PlayerNameContainer : NetworkBehaviour
    {
        // Local storage - not networked, no size limits
        private readonly Dictionary<PlayerRef, string> localPlayerNames = new();

        public event Action<List<string>> OnPlayerNamesChanged;

        private void UpdateUI()
        {
            var nameList = localPlayerNames.Values.ToList();
            OnPlayerNamesChanged?.Invoke(nameList);
        }

        // RPC to sync player name to all clients
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SyncPlayerNameRPC(string playerName, RpcInfo info = default)
        {
            localPlayerNames[info.Source] = playerName;
            Debug.Log($"Player {info.Source} set name to: {playerName}");
            UpdateUI();
        }

        // RPC to remove player from all clients
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RemovePlayerNameRPC(RpcInfo info = default)
        {
            if (!localPlayerNames.Remove(info.Source)) return;
            UpdateUI();
        }

        // RPC for late-joining players to request current state
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RequestCurrentPlayersRPC(RpcInfo info = default)
        {
            foreach (var kvp in localPlayerNames)
            {
                SyncPlayerNameToSingleClientRPC(kvp.Key, kvp.Value, info.Source);
            }
        }

        // RPC to send player name to a specific client
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void SyncPlayerNameToSingleClientRPC(PlayerRef playerRef, string playerName,
            [RpcTarget] PlayerRef target)
        {
            localPlayerNames[playerRef] = playerName;
            UpdateUI();
        }

        // Public method to add a player (called locally)
        public void AddPlayer(string playerName) =>
            SyncPlayerNameRPC(playerName);

        public void RemovePlayer(PlayerRef playerRef)
        {
            if (Runner.LocalPlayer == playerRef || Object.HasStateAuthority)
            {
                RemovePlayerNameRPC();
            }
        }

        public List<string> GetPlayerNames() => localPlayerNames.Values.ToList();

        public string GetPlayerName(PlayerRef playerRef) =>
            localPlayerNames.TryGetValue(playerRef, out string name) ? name : string.Empty;

        public Dictionary<PlayerRef, string> GetAllPlayerEntries() => new(localPlayerNames);

        public bool HasPlayer(PlayerRef playerRef) => localPlayerNames.ContainsKey(playerRef);

        public int GetPlayerCount() => localPlayerNames.Count;

        public override void Spawned()
        {
            LobbyManager.Instance.SetPlayerNameContainer(this);

            if (Object.HasStateAuthority)
            {
                Debug.Log("PlayerNameContainer spawned as State Authority");
            }
            else
            {
                Debug.Log("PlayerNameContainer spawned as client - requesting current players");
                RequestCurrentPlayersRPC();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // Clean up when despawned
            localPlayerNames.Clear();
            OnPlayerNamesChanged?.Invoke(new List<string>());
        }

        // Handle player disconnections
        public void OnPlayerDisconnected(PlayerRef player)
        {
            if (localPlayerNames.Remove(player))
            {
                Debug.Log($"Removed disconnected player: {player}");
                UpdateUI();
            }
        }
    }
}
