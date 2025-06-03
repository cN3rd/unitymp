using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW1.Scripts
{
    public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner runnerPrefab;

        [Header("Top view GUI")]
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_Dropdown lobbyDropdown;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button newRoomButton;

        [Header("List view")]
        [SerializeField] private RectTransform listPanel;
        [SerializeField] private GameObject listItemTemplate;
        [SerializeField] private TextMeshProUGUI noLobbySelectedText;
        [SerializeField] private TextMeshProUGUI noRoomsText;

        [Header("Popups")]
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private CreateRoomPopup createRoomPopup;
        
        [Header("Name list")]
        [SerializeField] private PlayerListUI playerListUI;
        [SerializeField] private GameObject nameContainerPrefab;

        public static LobbyManager Instance { get; private set; }
        
        private bool _canEnableJoinLobbyButton = true;
        private readonly List<GameObject> _roomButtons = new();
        private List<SessionInfo> _sessions = new();

        private NetworkRunner lobbyRunner;
        private NetworkRunner sessionRunner;
        private PlayerNameContainer playerNameContainer;

        public void Start()
        {
            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
            newRoomButton.onClick.AddListener(OnNewRoomButtonClicked);
            lobbyDropdown.onValueChanged.AddListener(OnLobbyDropdownValueChanged);
            createRoomPopup.OnCreateRoom += CreateRoom;
            
            Instance = this;
            UpdateUIState();
        }

        private void OnDestroy()
        {
            lobbyRunner?.RemoveCallbacks(this);
            sessionRunner?.RemoveCallbacks(this);
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _sessions = new List<SessionInfo>(sessionList);
            UpdateUIState();
        }

        public void OnConnectedToServer(NetworkRunner runner) => UpdateUIState();

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"Disconnected from server: {reason}");

            if (runner == lobbyRunner)
                _sessions.Clear();

            UpdateUIState();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => playerNameContainer.NotifyPlayerJoinRPC(playerNameInputField.text);
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => playerNameContainer.NotifyPlayerLeaveRPC();

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectRequest(NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress,
            NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key,
            float progress) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner,
            Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        private string GetLobbyName(int option) => $"lobby{option}";

        private string GetCurrentLobbyName() => GetLobbyName(lobbyDropdown.value);

        private void OnNewRoomButtonClicked() => createRoomPopup.ShowPopup();

        private async void CreateRoom(CreateRoomDetails details)
        {
            try
            {
                if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                sessionRunner = Instantiate(runnerPrefab);

                StartGameResult result = await sessionRunner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = details.name,
                    PlayerCount = details.numPlayers,
                    CustomLobbyName = lobbyRunner?.LobbyInfo.IsValid == true
                        ? lobbyRunner.LobbyInfo.Name
                        : GetCurrentLobbyName()
                });

                // TODO
                await sessionRunner.SpawnAsync(nameContainerPrefab);
                
                if (!result.Ok)
                {
                    errorPopup.ShowError(result.ErrorMessage);
                    Debug.LogError($"Failed to create room: {result.ErrorMessage}");

                    if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                    sessionRunner = null;
                }
                else
                {
                    Debug.Log($"Successfully created room: {details.name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while creating room: {ex.Message}");
                errorPopup.ShowError($"Failed to create room: {ex.Message}");
            }

            UpdateUIState();
        }

        public void SetPlayerNameContainer(PlayerNameContainer pnc)
        {
            playerNameContainer = pnc;
            playerNameContainer.OnPlayerNamesChanged += playerListUI.RefreshPlayerList;
        }

        private void OnLobbyDropdownValueChanged(int newValue) => UpdateUIState();

        #region UI
        private void UpdateUIState()
        {
            bool isLobbyConnected = lobbyRunner?.LobbyInfo.IsValid == true;
            bool isCurrentLobby =
                isLobbyConnected && lobbyRunner.LobbyInfo.Name == GetCurrentLobbyName();

            bool isInSession = sessionRunner?.IsRunning == true;

            UpdateButtonStateUI(isCurrentLobby, isInSession);
            UpdateListTextUI(isLobbyConnected);
            UpdateListUI(isLobbyConnected, isInSession);
        }
        
        private void UpdateListTextUI(bool isLobbyConnected)
        {
            noLobbySelectedText.gameObject.SetActive(!isLobbyConnected);
            noRoomsText.gameObject.SetActive(isLobbyConnected && _sessions?.Count == 0);
        }
        
        private void UpdateButtonStateUI(bool isCurrentLobby, bool isInSession)
        {
            newRoomButton.interactable = isCurrentLobby && !isInSession;
            joinLobbyButton.interactable =
                _canEnableJoinLobbyButton && (lobbyRunner == null || !isCurrentLobby);
        }

        private void UpdateListUI(bool isLobbyConnected, bool isInSession)
        {
            foreach (GameObject button in _roomButtons)
            {
                if (button) Destroy(button);
            }
            _roomButtons.Clear();

            if (_sessions == null || !isLobbyConnected) return;

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

            if (isInSession && session.Name == sessionRunner.SessionInfo?.Name)
            {
                // If we're in a session, show disconnect option
                imageComponent.color = Color.green;
                button.onClick.AddListener(DisconnectSession);
                buttonText.text += " [DISCONNECT]";
            }
            else if (session.IsOpen && session.PlayerCount < session.MaxPlayers)
            {
                // Session is joinable
                button.onClick.AddListener(() => JoinSession(session));
            }
            else
            {
                // Session is full or closed
                button.interactable = false;
            }

            _roomButtons.Add(sessionObject);
        }
        #endregion
        
        private async void DisconnectSession()
        {
            try
            {
                if (sessionRunner != null)
                {
                    await sessionRunner.Shutdown(shutdownReason: ShutdownReason.Ok);
                    if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                    sessionRunner = null;
                }

                UpdateUIState();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while disconnecting session: {ex.Message}");
            }
        }

        private async void JoinSession(SessionInfo session)
        {
            try
            {
                if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                sessionRunner = Instantiate(runnerPrefab);

                StartGameResult result = await sessionRunner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = session.Name,
                    CustomLobbyName = lobbyRunner.LobbyInfo.Name
                });

                if (!result.Ok)
                {
                    errorPopup.ShowError(result.ErrorMessage);
                    Debug.LogError($"Failed to join session: {result.ErrorMessage}");

                    if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                    sessionRunner = null;
                }
                else
                {
                    Debug.Log($"Successfully joined session: {session.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while joining session: {ex.Message}");
                errorPopup.ShowError($"Failed to join session: {ex.Message}");
            }

            UpdateUIState();
        }

        private async void OnJoinLobbyClicked()
        {
            try
            {
                if (sessionRunner?.gameObject) Destroy(sessionRunner.gameObject);
                sessionRunner = null;

                if (lobbyRunner != null)
                {
                    lobbyRunner.RemoveCallbacks(this);
                    if (lobbyRunner.gameObject) Destroy(lobbyRunner.gameObject);
                }

                lobbyRunner = Instantiate(runnerPrefab);
                lobbyRunner.AddCallbacks(this);

                _canEnableJoinLobbyButton = false;
                UpdateUIState();

                StartGameResult result =
                    await lobbyRunner.JoinSessionLobby(SessionLobby.Custom, GetCurrentLobbyName());

                _canEnableJoinLobbyButton = true;

                if (!result.Ok)
                {
                    errorPopup.ShowError(result.ErrorMessage);
                    Debug.LogError($"Failed to join lobby: {result.ErrorMessage}");
                }
                else
                {
                    Debug.Log($"Successfully joined lobby: {GetCurrentLobbyName()}");
                }
            }
            catch (Exception ex)
            {
                _canEnableJoinLobbyButton = true;
                Debug.LogError($"Exception while joining lobby: {ex.Message}");
                errorPopup.ShowError($"Failed to join lobby: {ex.Message}");
            }

            UpdateUIState();
        }
    }
}
