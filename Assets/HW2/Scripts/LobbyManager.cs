using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW2.Scripts
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private GameObject playerDataPrefab;

        [Header("Top view GUI")]
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_Dropdown lobbyDropdown;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button newRoomButton;

        [Header("List view")] [SerializeField] private RectTransform listPanel;
        [SerializeField] private GameObject listItemTemplate;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Popups")]
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private CreateRoomPopup createRoomPopup;

        [Header("Name list")]
        [SerializeField] private PlayerListUI playerListUI;
        [SerializeField] private GameObject nameContainerPrefab;
        
        private readonly List<GameObject> _roomButtons = new();

        // state tracking
        private bool _canEnableJoinLobbyButton = true;
        private bool _isJoiningLobby;
        private bool _isJoiningSession;

        // lobby things
        private LobbyNetworkCallbackHandler _lobbyCallbackHandler;
        private NetworkRunner _lobbyRunner;
        private PlayerNameContainer _playerNameContainer;

        // network runner things
        private SessionNetworkCallbackHandler _sessionCallbackHandler;
        private NetworkRunner _sessionRunner;
        private List<SessionInfo> _sessions = new();

        public static LobbyManager Instance { get; private set; }

        public void Start()
        {
            Instance = this;

            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
            newRoomButton.onClick.AddListener(OnNewRoomButtonClicked);
            lobbyDropdown.onValueChanged.AddListener(OnLobbyDropdownValueChanged);
            createRoomPopup.OnCreateRoom += CreateRoom;

            UpdateUIState();
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
            if (_sessionRunner != null)
            {
                if (_sessionCallbackHandler != null)
                {
                    _sessionRunner.RemoveCallbacks(_sessionCallbackHandler);
                    _sessionCallbackHandler = null;
                }

                if (_sessionRunner.gameObject)
                {
                    Destroy(_sessionRunner.gameObject);
                }

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
                UpdateUIState();
            }
            catch (Exception ex)
            {
                errorPopup.ShowError(ex.ToString());
                CleanupSessionConnection();
                UpdateUIState();
            }
        }

        #endregion

        #region Lobby Network Event Handlers

        private void HandleLobbySessionListUpdated(List<SessionInfo> sessionList)
        {
            _sessions = new List<SessionInfo>(sessionList);
            UpdateUIState();
        }

        private void HandleLobbyConnected() => UpdateUIState();

        private void HandleLobbyDisconnected(NetDisconnectReason reason)
        {
            _sessions.Clear();
            UpdateUIState();
        }

        #endregion

        #region Session Network Event Handlers

        private void HandleSessionConnected() => UpdateUIState();

        private void HandleSessionDisconnected(NetDisconnectReason reason) => UpdateUIState();

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

        private async void OnJoinLobbyClicked()
        {
            try
            {
                _canEnableJoinLobbyButton = false;
                _isJoiningLobby = true;
                UpdateUIState();

                CleanupSessionConnection();
                CleanupLobbyConnection();
                CreateLobbyRunner();
                StartGameResult result =
                    await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom, GetCurrentLobbyName());

                _isJoiningLobby = false;
                _canEnableJoinLobbyButton = true;

                if (!result.Ok)
                {
                    errorPopup.ShowError(result.ErrorMessage);
                    CleanupLobbyConnection();
                }
            }
            catch (Exception ex)
            {
                _isJoiningLobby = false;
                _canEnableJoinLobbyButton = true;
                errorPopup.ShowError($"Failed to join lobby: {ex.Message}");
                CleanupLobbyConnection();
            }

            UpdateUIState();
        }

        private async void CreateRoom(CreateRoomDetails details)
        {
            try
            {
                _isJoiningSession = true;
                CleanupSessionConnection();
                CreateSessionRunner();

                StartGameResult result = await _sessionRunner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = details.name,
                    PlayerCount = details.numPlayers,
                    CustomLobbyName = _lobbyRunner?.LobbyInfo.IsValid == true
                        ? _lobbyRunner.LobbyInfo.Name
                        : GetCurrentLobbyName()
                });

                if (!result.Ok)
                {
                    errorPopup.ShowError(result.ErrorMessage);
                    CleanupSessionConnection();
                    return;
                }

                await _sessionRunner.SpawnAsync(nameContainerPrefab);
                _isJoiningSession = false;
            }
            catch (Exception ex)
            {
                errorPopup.ShowError($"Failed to create room: {ex.Message}");
                CleanupSessionConnection();
                _isJoiningSession = false;
            }

            UpdateUIState();
        }

        private PlayerData SpawnPlayerData()
        {
            NetworkObject playerDataObject = _sessionRunner.Spawn(playerDataPrefab);
            _sessionRunner.SetPlayerObject(_sessionRunner.LocalPlayer, playerDataObject);

            PlayerData playerData = playerDataObject.GetComponent<PlayerData>();
            playerData.Nickname = playerNameInputField.text;

            return playerData;
        }

        private async void JoinSession(SessionInfo session)
        {
            try
            {
                _isJoiningSession = true;
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
                    errorPopup.ShowError(result.ErrorMessage);
                    CleanupSessionConnection();
                }
                
                _isJoiningSession = false;
            }
            catch (Exception ex)
            {
                errorPopup.ShowError($"Failed to join session: {ex.Message}");
                CleanupSessionConnection();
                _isJoiningSession = false;
            }

            UpdateUIState();
        }

        private void DisconnectSession() => ShutdownSessionSafely();

        #endregion

        #region UI Event Handlers

        private void OnNewRoomButtonClicked() => createRoomPopup.ShowPopup();

        private void OnLobbyDropdownValueChanged(int newValue) => UpdateUIState();

        public void SetPlayerNameContainer(PlayerNameContainer pnc)
        {
            _playerNameContainer = pnc;
            _playerNameContainer.OnPlayerListChanged += OnPlayerListChanged;
            SpawnPlayerData();
        }

        private void OnPlayerListChanged(List<string> playerList) =>
            playerListUI.RefreshPlayerList(playerList);

        #endregion

        #region UI Updates

        private void UpdateUIState()
        {
            bool isLobbyConnected = _lobbyRunner?.LobbyInfo.IsValid == true;
            bool isCurrentLobby =
                isLobbyConnected && _lobbyRunner.LobbyInfo.Name == GetCurrentLobbyName();

            bool isInSession = _sessionRunner?.IsRunning == true;

            UpdateButtonStateUI(isCurrentLobby, isInSession);
            UpdateListTextUI(isLobbyConnected);
            UpdateListUI(isLobbyConnected, isInSession);
        }

        private void UpdateListTextUI(bool isLobbyConnected)
        {
            if (_isJoiningLobby)
            {
                statusText.text = "Joining lobby...";
                statusText.gameObject.SetActive(true);
            }
            else if (_isJoiningSession)
            {
                statusText.text = "Joining room...";
                statusText.gameObject.SetActive(true);
            }
            else if (!isLobbyConnected)
            {
                statusText.text = "Select a lobby and click 'Join Lobby' to see available rooms";
                statusText.gameObject.SetActive(true);
            }
            else if (_sessions.Count == 0)
            {
                statusText.text = "No rooms available. Click 'New Room' to create one.";
                statusText.gameObject.SetActive(true);
            }
            else
            {
                statusText.gameObject.SetActive(false);
            }
        }

        private void UpdateButtonStateUI(bool isCurrentLobby, bool isInSession)
        {
            newRoomButton.interactable = isCurrentLobby && !isInSession;
            joinLobbyButton.interactable =
                _canEnableJoinLobbyButton && (_lobbyRunner == null || !isCurrentLobby);
        }

        private void UpdateListUI(bool isLobbyConnected, bool isInSession)
        {
            foreach (GameObject button in _roomButtons)
            {
                if (button) Destroy(button);
            }

            _roomButtons.Clear();

            if (!isLobbyConnected) return;

            foreach (SessionInfo session in _sessions)
            {
                GenerateButtonUI(session, isInSession);
            }
        }

        private void GenerateButtonUI(SessionInfo session, bool isInSession)
        {
            GameObject sessionObject = Instantiate(listItemTemplate, listPanel);
            sessionObject.SetActive(true);

            TextMeshProUGUI buttonText = sessionObject.GetComponentInChildren<TextMeshProUGUI>();
            Image imageComponent = sessionObject.GetComponent<Image>();
            Button button = sessionObject.GetComponent<Button>();

            buttonText.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";

            if (isInSession && session.Name == _sessionRunner.SessionInfo?.Name)
            {
                imageComponent.color = Color.green;
                button.onClick.AddListener(DisconnectSession);
                buttonText.text += " [DISCONNECT]";
            }
            else if (session.IsOpen && session.PlayerCount < session.MaxPlayers)
            {
                button.onClick.AddListener(() => JoinSession(session));
            }
            else
            {
                button.interactable = false;
            }

            _roomButtons.Add(sessionObject);
        }

        #endregion

        #region Utility Methods

        private string GetLobbyName(int option) => $"lobby{option}";
        private string GetCurrentLobbyName() => GetLobbyName(lobbyDropdown.value);

        #endregion
    }
}
