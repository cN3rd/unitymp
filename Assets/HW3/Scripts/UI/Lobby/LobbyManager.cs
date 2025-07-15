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
            createRoomPopup.OnCreateRoom += CreateRoom;
        }

        private void Start()
        {
            Instance = this;
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (!_sessionRunner || !_sessionRunner.IsInSession)
                CleanupSessionConnection();
            CleanupLobbyConnection();

            if (_playerNameContainer)
            {
                _playerNameContainer.OnPlayerListChanged -= OnPlayerListChanged;
            }
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
            if (_lobbyRunner != null)
            {
                if (_lobbyCallbackHandler != null)
                {
                    _lobbyRunner.RemoveCallbacks(_lobbyCallbackHandler);
                    _lobbyCallbackHandler = null;
                }

                if (_lobbyRunner.gameObject)
                {
                    Destroy(_lobbyRunner.gameObject);
                }

                _lobbyRunner = null;
            }

            _sessions.Clear();
        }

        private void CleanupSessionConnection()
        {
            if (_sessionCallbackHandler != null)
            {
                _sessionRunner.RemoveCallbacks(_sessionCallbackHandler);
                _sessionCallbackHandler = null;
            }
            
            if (_sessionRunner?.gameObject)
            {
                Destroy(_sessionRunner.gameObject);
                _sessionRunner = null;
            }

            _playerNameContainer = null;
        }

        private async void ShutdownSessionSafely()
        {
            try
            {
                if (_sessionRunner == null) return;

                await _sessionRunner.Shutdown(shutdownReason: ShutdownReason.Ok);
                CleanupSessionConnection();
                RefreshUI();
            }
            catch (Exception ex)
            {
                errorPopup.ShowError(ex.ToString());
                CleanupSessionConnection();
                RefreshUI();
            }
        }

        #endregion

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
                _sessionRunner.IsSceneAuthority) _sessionRunner.LoadScene("GameScene");
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

                CleanupSessionConnection();
                CleanupLobbyConnection();
                CreateLobbyRunner();
                StartGameResult result =
                    await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom,
                        lobbyManagerUI.GetCurrentLobbyName());

                _lobbyState = LobbyState.InLobby;

                if (!result.Ok)
                {
                    _lobbyState = LobbyState.NotConnected;
                    errorPopup.ShowError($"Failed to join lobby: {result.ErrorMessage}");
                    CleanupLobbyConnection();
                }


            RefreshUI();
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
            try
            {
                _lobbyState = LobbyState.ConnectingToSession;
                CleanupSessionConnection();
                CreateSessionRunner();

                StartGameResult result = await _sessionRunner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = details.name,
                    PlayerCount = details.numPlayers,
                    CustomLobbyName = _lobbyRunner?.LobbyInfo.IsValid == true
                        ? _lobbyRunner.LobbyInfo.Name
                        : lobbyManagerUI.GetCurrentLobbyName()
                });
                _lobbyState = LobbyState.CreatingNCP;

                if (!result.Ok)
                {
                    throw new Exception("Failed to create room");
                }

                await _sessionRunner.SpawnAsync(nameContainerPrefab);
                _lobbyState = LobbyState.InSession;
            }
            catch (Exception ex)
            {
                errorPopup.ShowError($"Failed to create room: {ex.Message}");
                CleanupSessionConnection();
                _lobbyState = LobbyState.InLobby;
            }

            RefreshUI();
        }

        
        private async void JoinSession(SessionInfo session)
        {
            try
            {
                _lobbyState = LobbyState.InSession;
                CleanupSessionConnection();
                CreateSessionRunner();

                StartGameResult result = await _sessionRunner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = session.Name,
                    CustomLobbyName = _lobbyRunner.LobbyInfo.Name
                });

                if (!result.Ok)
                {
                    throw new Exception(result.ErrorMessage);
                }
                
                _lobbyState = LobbyState.InSession;
            }
            catch (Exception ex)
            {
                errorPopup.ShowError($"Failed to join session: {ex.Message}");
                CleanupSessionConnection();
                _lobbyState = LobbyState.InLobby;
            }

            RefreshUI();
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
            playerListUI?.RefreshPlayerList(playerList);
        
        private void RefreshUI() => lobbyManagerUI.UpdateUIState(_lobbyState,
            _lobbyRunner?.LobbyInfo, _sessions, _sessionRunner?.SessionInfo);
        
        #endregion
    }
}
