using TMPro;
using UnityEngine;
using HW2.Scripts; // So PlayerData is recognized

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inputContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI chatHistoryText;

    private bool isChatOpen = false;
    private PlayerData owner; 

    public void SetOwner(PlayerData player) 
    {
        owner = player;
    }

    private void Awake()
    {
        inputContainer.SetActive(false);
    }

    private void Update()
    {
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
        string message = inputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            owner?.SendChatMessage(message); 
        }
    }

    public void ReceiveMessageLocally(string message)
    {
        chatHistoryText.text += message + "\n";
    }
}
