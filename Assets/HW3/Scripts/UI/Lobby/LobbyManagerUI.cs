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
        public event UnityAction<bool> OnRoomVisibleValueChange;

        [Header("Top view GUI")]
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_Dropdown lobbyDropdown;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button newRoomButton;

        [Header("List view")] 
        [SerializeField] private RectTransform listPanel;
        [SerializeField] private Toggle roomVisibleToggle;
        [SerializeField] private GameObject listItemTemplate;
        [SerializeField] private TextMeshProUGUI statusText;
        private readonly List<GameObject> _roomButtons = new();

        public string GetCurrentLobbyName() => LobbyManagerUtils.GetLobbyName(lobbyDropdown.value);
        public string GetPlayerName() => playerNameInputField.text;

        private void Start()
        {
            joinLobbyButton.onClick.AddListener(() => OnJoinLobbyClicked?.Invoke());
            newRoomButton.onClick.AddListener(() => OnNewRoomButtonClicked?.Invoke());
            lobbyDropdown.onValueChanged.AddListener(value => OnLobbyDropdownValueChanged?.Invoke(value));
            roomVisibleToggle.onValueChanged.AddListener(value => OnRoomVisibleValueChange?.Invoke(value));
        }

        public void UpdateUIState(LobbyState state, LobbyInfo lobbyInfo, List<SessionInfo> sessions,
            SessionInfo currentSession)
        {
            bool isLobbyConnected = state != LobbyState.NotConnected;
            bool isCurrentLobby = isLobbyConnected && lobbyInfo?.Name == GetCurrentLobbyName();
            bool inSession = currentSession != null;
            int numSessions = sessions.Count;

            UpdateStatusText(state, numSessions);
            UpdateRoomVisibilityToggle(state, currentSession);
            UpdateButtonsState(state, isCurrentLobby);
            RebuildRoomList(isLobbyConnected, sessions, currentSession);
        }

        private void UpdateRoomVisibilityToggle(LobbyState state, SessionInfo currentSession)
        {
            if (state != LobbyState.InSession || currentSession == null)
            {
                roomVisibleToggle.gameObject.SetActive(false);
                return;
            }
            
            roomVisibleToggle.gameObject.SetActive(true);
            roomVisibleToggle.SetIsOnWithoutNotify(currentSession.IsVisible);
        }


        private void UpdateStatusText(LobbyState state, int numSessions)
        {
            (string text, bool isActive) = state switch
            {
                LobbyState.NotConnected => ("Select a lobby and click 'Join Lobby' to see available rooms", true),
                LobbyState.ConnectingToLobby => ("Joining lobby...", true),
                LobbyState.ConnectingToSession => ("Joining room...", true),
                LobbyState.InLobby when numSessions == 0 => ("No rooms available. Click 'New Room' to create one.", true),
                _ => ("", false)
            };

            statusText.text = text;
            statusText.gameObject.SetActive(isActive);
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

            // Always show current session first if it exists
            if (currentSession != null)
            {
                GenerateButtonUI(currentSession, true);
            }

            // Show other sessions from the lobby
            foreach (SessionInfo session in sessions)
            {
                if (currentSession != null && session.Name == currentSession.Name) continue; // skip current session
                GenerateButtonUI(session, false);
            }
        }

        private void GenerateButtonUI(SessionInfo session, bool isCurrentSession)
        {
            // For current session, always show regardless of session.IsVisible from the list
            // For other sessions, only show if visible
            if (!isCurrentSession && !session.IsVisible) return;
            
            GameObject sessionObject = Instantiate(listItemTemplate, listPanel);
            sessionObject.SetActive(true);

            TextMeshProUGUI buttonText = sessionObject.GetComponentInChildren<TextMeshProUGUI>();
            Image imageComponent = sessionObject.GetComponent<Image>();
            Button button = sessionObject.GetComponent<Button>();

            buttonText.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";

            if (session.IsOpen && isCurrentSession)
            {
                imageComponent.color = Color.green;
                button.onClick.AddListener(() => OnDisconnectSession?.Invoke());
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
