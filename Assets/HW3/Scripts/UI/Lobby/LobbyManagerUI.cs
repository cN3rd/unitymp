using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class LobbyManagerUI : MonoBehaviour
    {
        public event UnityAction<SessionInfo> OnJoinSession;
        public event UnityAction OnDisconnectSession;
        public event UnityAction OnJoinLobbyClicked;
        public event UnityAction OnNewRoomButtonClicked;
        public event UnityAction<int> OnLobbyDropdownValueChanged;

        [Header("Top view GUI")]
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_Dropdown lobbyDropdown;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button newRoomButton;

        [Header("List view")] [SerializeField] private RectTransform listPanel;
        [SerializeField] private GameObject listItemTemplate;
        [SerializeField] private TextMeshProUGUI statusText;
        private readonly List<GameObject> _roomButtons = new();

        public string GetCurrentLobbyName() => LobbyManagerUtils.GetLobbyName(lobbyDropdown.value);
        public string GetPlayerName() => playerNameInputField.text;

        public void Start()
        {
            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
            newRoomButton.onClick.AddListener(OnNewRoomButtonClicked);
            lobbyDropdown.onValueChanged.AddListener(OnLobbyDropdownValueChanged);
        }

        public void UpdateUIState(LobbyState state, LobbyInfo lobbyInfo, List<SessionInfo> sessions,
            SessionInfo currentSession)
        {
            bool isLobbyConnected = state != LobbyState.NotConnected;
            bool isCurrentLobby = isLobbyConnected && lobbyInfo?.Name == GetCurrentLobbyName();
            int numSessions = sessions.Count;

            UpdateStatusText(state, numSessions);
            UpdateButtonsState(state, isCurrentLobby);
            RebuildRoomList(isLobbyConnected, sessions, currentSession);
        }

        private void UpdateStatusText(LobbyState state, int numSessions)
        {
            switch (state)
            {
                case LobbyState.NotConnected:
                    statusText.text = "Select a lobby and click 'Join Lobby' to see available rooms";
                    statusText.gameObject.SetActive(true);
                    break;
                case LobbyState.ConnectingToLobby:
                    statusText.text = "Joining lobby...";
                    statusText.gameObject.SetActive(true);
                    break;
                case LobbyState.ConnectingToSession:
                    statusText.text = "Joining room...";
                    statusText.gameObject.SetActive(true);
                    break;
                default:
                {
                    if (numSessions == 0)
                    {
                        statusText.text = "No rooms available. Click 'New Room' to create one.";
                        statusText.gameObject.SetActive(true);
                    }
                    else
                    {
                        statusText.gameObject.SetActive(false);
                    }

                    break;
                }
            }
        }

        private void UpdateButtonsState(LobbyState state, bool isCurrentLobby)
        {
            newRoomButton.interactable = state == LobbyState.InLobby && isCurrentLobby;
            joinLobbyButton.interactable =
                state is not (LobbyState.ConnectingToLobby or LobbyState.ConnectingToSession) &&
                !isCurrentLobby;
        }

        private void RebuildRoomList(bool isLobbyConnected, List<SessionInfo> sessions,
            SessionInfo currentSession)
        {
            foreach (GameObject button in _roomButtons)
            {
                if (button) Destroy(button);
            }

            _roomButtons.Clear();

            if (!isLobbyConnected) return;

            foreach (SessionInfo session in sessions)
            {
                GenerateButtonUI(session, currentSession);
            }
        }

        private void GenerateButtonUI(SessionInfo session, SessionInfo currentSession)
        {
            GameObject sessionObject = Instantiate(listItemTemplate, listPanel);
            sessionObject.SetActive(true);

            TextMeshProUGUI buttonText = sessionObject.GetComponentInChildren<TextMeshProUGUI>();
            Image imageComponent = sessionObject.GetComponent<Image>();
            Button button = sessionObject.GetComponent<Button>();

            buttonText.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";

            if (currentSession.IsValid && session.Name == currentSession.Name)
            {
                imageComponent.color = Color.green;
                button.onClick.AddListener(OnDisconnectSession);
                buttonText.text += " [DISCONNECT]";
            }
            else if (session.IsOpen && session.PlayerCount < session.MaxPlayers)
            {
                button.onClick.AddListener(() => OnJoinSession?.Invoke(session));
            }
            else
            {
                button.interactable = false;
            }

            _roomButtons.Add(sessionObject);
        }
    }
}
