using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace HW3.Scripts
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private GameObject playerDataPrefab;

        [SerializeField] private LobbyManagerUI lobbyManagerUI;

        [Header("Popups")]
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private CreateRoomPopup createRoomPopup;

        [Header("Name list")]
        [SerializeField] private PlayerListUI playerListUI;
        [SerializeField] private GameObject nameContainerPrefab;

        private LobbyState _lobbyState = LobbyState.NotConnected;

        // lobby things
        private LobbyNetworkCallbackHandler _lobbyCallbackHandler;
        private NetworkRunner _lobbyRunner;
        private PlayerNameContainer _playerNameContainer;

        // network runner things
        private SessionNetworkCallbackHandler _sessionCallbackHandler;
        private NetworkRunner _sessionRunner;
        private List<SessionInfo> _sessions = new();

        public static LobbyManager Instance { get; private set; }

        private void Awake()
        {
            lobbyManagerUI.OnJoinSession += JoinSession;
            lobbyManagerUI.OnDisconnectSession += DisconnectSession;
            lobbyManagerUI.OnJoinLobbyClicked += JoinLobby;
            lobbyManagerUI.OnNewRoomButtonClicked += createRoomPopup.ShowPopup;
            lobbyManagerUI.OnLobbyDropdownValueChanged += _ => RefreshUI();
            lobbyManagerUI.OnRoomVisibleValueChange += ChangeRoomVisibility;
            lobbyManagerUI.OnError += errorPopup.ShowError;
            createRoomPopup.OnCreateRoom += CreateRoom;
        }

        private void ChangeRoomVisibility(bool newVisibility)
        {
            if (!_sessionRunner) return;
            if (!_sessionRunner.SessionInfo) return;
            if (!_sessionRunner.SessionInfo.IsValid) return;

            _sessionRunner.SessionInfo.IsVisible = newVisibility;
        }

        private void Start()
        {
            Instance = this;
            RefreshUI();
        }

        private void OnDestroy()
        {
            CleanupAllConnections();

            if (_playerNameContainer)
                _playerNameContainer.OnPlayerListChanged -= OnPlayerListChanged;
        }

        #region Creation Operations

        private void CreateLobbyRunner()
        {
            _lobbyRunner = Instantiate(runnerPrefab);
            _lobbyCallbackHandler = new LobbyNetworkCallbackHandler(
                HandleLobbySessionListUpdated,
                HandleLobbyConnected,
                HandleLobbyDisconnected
            );

            _lobbyRunner.AddCallbacks(_lobbyCallbackHandler);
        }

        private void CreateSessionRunner()
        {
            _sessionRunner = Instantiate(runnerPrefab);
            _sessionCallbackHandler = new SessionNetworkCallbackHandler(
                HandleSessionConnected,
                HandleSessionDisconnected,
                HandleSessionPlayerJoined,
                HandleSessionPlayerLeft
            );

            _sessionRunner.AddCallbacks(_sessionCallbackHandler);
        }

        #endregion

        #region Cleanup Operations

        private void CleanupAllConnections()
        {
            CleanupLobbyConnection();
            CleanupSessionConnection();
        }

        private void CleanupLobbyConnection()
        {
            _lobbyRunner?.RemoveCallbacks(_lobbyCallbackHandler);
            _lobbyCallbackHandler = null;

            if (_lobbyRunner?.gameObject)
                Destroy(_lobbyRunner.gameObject);
            _lobbyRunner = null;

            _sessions.Clear();
        }

        private void CleanupSessionConnection()
        {
            _sessionRunner?.RemoveCallbacks(_sessionCallbackHandler);
            _sessionCallbackHandler = null;

            if (_sessionRunner?.gameObject)
                Destroy(_sessionRunner.gameObject);

            _sessionRunner = null;
            _playerNameContainer = null;
        }

        private async void ShutdownSessionSafely()
        {
            try
            {
                if (_sessionRunner == null) return;
                await _sessionRunner.Shutdown(shutdownReason: ShutdownReason.Ok);
            }
            catch (Exception ex)
            {
                errorPopup.ShowError(ex.ToString());
            }
            finally
            {
                CleanupSessionConnection();
                RefreshUI();
                _lobbyState = LobbyState.InLobby;
            }
        }

        #endregion

        private async Awaitable ExecuteNetworkOperation(
            Func<Awaitable> operation,
            string errorPrefix,
            LobbyState? successState = null,
            LobbyState? failureState = null,
            Action onFailure = null)
        {
            try
            {
                await operation();
                if (successState.HasValue)
                    _lobbyState = successState.Value;
            }
            catch (Exception ex)
            {
                errorPopup.ShowError($"{errorPrefix}: {ex.Message}");
                if (failureState.HasValue)
                    _lobbyState = failureState.Value;
                onFailure?.Invoke();
            }
            finally
            {
                RefreshUI();
            }
        }
        
        #region Lobby Network Event Handlers

        private void HandleLobbySessionListUpdated(List<SessionInfo> sessionList)
        {
            _sessions = new List<SessionInfo>(sessionList);
            RefreshUI();
        }

        private void HandleLobbyConnected() => RefreshUI();

        private void HandleLobbyDisconnected(NetDisconnectReason reason)
        {
            _sessions.Clear();
            RefreshUI();
        }

        #endregion

        #region Session Network Event Handlers

        private void HandleSessionConnected() => RefreshUI();

        private void HandleSessionDisconnected(NetDisconnectReason reason) => RefreshUI();

        private void HandleSessionPlayerJoined(PlayerRef player)
        {
            if (_sessionRunner.SessionInfo.MaxPlayers == _sessionRunner.SessionInfo.PlayerCount &&
                _sessionRunner.IsSceneAuthority)
            {
                _sessionRunner.SessionInfo.IsOpen = false;
                _sessionRunner.LoadScene("GameScene");
            }
        }

        private void HandleSessionPlayerLeft(PlayerRef player)
        {
            /* Intentionally empty */
        }

        #endregion

        #region Network Operations

        private async void JoinLobby()
        {
            _lobbyState = LobbyState.ConnectingToLobby;
            RefreshUI();

            await ExecuteNetworkOperation(
                operation: async () =>
                {
                    CleanupSessionConnection();
                    CleanupLobbyConnection();
                    CreateLobbyRunner();
                    var result = await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom,
                        lobbyManagerUI.GetCurrentLobbyName());

                    if (!result.Ok)
                        throw new Exception(result.ErrorMessage);
                },
                errorPrefix: "Failed to join lobby",
                successState: LobbyState.InLobby,
                failureState: LobbyState.NotConnected,
                onFailure: CleanupLobbyConnection
            );
        }

        private PlayerData SpawnPlayerData()
        {
            NetworkObject playerDataObject = _sessionRunner.Spawn(playerDataPrefab);
            _sessionRunner.SetPlayerObject(_sessionRunner.LocalPlayer, playerDataObject);

            PlayerData playerData = playerDataObject.GetComponent<PlayerData>();
            playerData.Nickname = lobbyManagerUI.GetPlayerName();

            return playerData;
        }

        private async void CreateRoom(CreateRoomDetails details)
        {
            _lobbyState = LobbyState.ConnectingToSession;
            RefreshUI();

            await ExecuteNetworkOperation(
                operation: async () =>
                {
                    CleanupSessionConnection();
                    CreateSessionRunner();

                    var result = await _sessionRunner.StartGame(new StartGameArgs
                    {
                        GameMode = GameMode.Shared,
                        SessionName = details.Name,
                        PlayerCount = details.NumPlayers,
                        CustomLobbyName = _lobbyRunner?.LobbyInfo.IsValid == true
                            ? _lobbyRunner.LobbyInfo.Name
                            : lobbyManagerUI.GetCurrentLobbyName()
                    });

                    if (!result.Ok)
                        throw new Exception("Failed to create room");

                    _lobbyState = LobbyState.CreatingNcp;
                    await _sessionRunner.SpawnAsync(nameContainerPrefab);
                },
                errorPrefix: "Failed to create room",
                successState: LobbyState.InSession,
                failureState: LobbyState.InLobby,
                onFailure: CleanupSessionConnection
            );
        }

        private async void JoinSession(SessionInfo session)
        {
            _lobbyState = LobbyState.ConnectingToSession;
            RefreshUI();

            await ExecuteNetworkOperation(
                operation: async () =>
                {
                    CleanupSessionConnection();
                    CreateSessionRunner();

                    var result = await _sessionRunner.StartGame(new StartGameArgs
                    {
                        GameMode = GameMode.Shared,
                        SessionName = session.Name,
                        CustomLobbyName = _lobbyRunner.LobbyInfo.Name
                    });

                    if (!result.Ok)
                        throw new Exception(result.ErrorMessage);
                },
                errorPrefix: "Failed to join session",
                successState: LobbyState.InSession,
                failureState: LobbyState.InLobby,
                onFailure: CleanupSessionConnection
            );
        }

        private void DisconnectSession() => ShutdownSessionSafely();

        #endregion

        #region UI Event Handlers

        public void SetPlayerNameContainer(PlayerNameContainer pnc)
        {
            _playerNameContainer = pnc;
            _playerNameContainer.OnPlayerListChanged += OnPlayerListChanged;
            SpawnPlayerData();
        }

        private void OnPlayerListChanged(List<string> playerList) =>
            playerListUI.RefreshPlayerList(playerList);

        private void RefreshUI() => lobbyManagerUI.UpdateUIState(_lobbyState,
            _lobbyRunner?.LobbyInfo, _sessions, _sessionRunner?.SessionInfo);

        #endregion
    }
}
