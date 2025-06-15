using TMPro;
using UnityEngine;
using Fusion;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject inputContainer;      // The panel containing the input field
    [SerializeField] private TMP_InputField inputField;      // Where the user types messages
    [SerializeField] private TextMeshProUGUI chatHistoryText; // Displays chat log

    private bool isChatOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (inputContainer != null)
            inputContainer.SetActive(false); // Hide chat input on start
    }

    private void Update()
    {
        if (!HasInputAuthority)
            return; // Only local player can open chat

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatOpen)
            {
                OpenChat();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(inputField.text))
                {
                    SendMessageToAll();
                }
                CloseChat();
            }
        }
    }

    private void OpenChat()
    {
        isChatOpen = true;
        inputContainer.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void CloseChat()
    {
        isChatOpen = false;
        inputContainer.SetActive(false);
        inputField.DeactivateInputField();
    }

    private void SendMessageToAll()
    {
        if (Runner == null)
        {
            Debug.LogError("Runner is null! Make sure ChatManager is attached to a spawned NetworkObject.");
            return;
        }

        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            Rpc_SendMessageToAll($"{Runner.LocalPlayer.PlayerId}: {message}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_SendMessageToAll(string message)
    {
        if (chatHistoryText == null)
        {
            Debug.LogError("chatHistoryText is null. Assign it in the Inspector.");
            return;
        }

        chatHistoryText.text += message + "\n";
    }
}
