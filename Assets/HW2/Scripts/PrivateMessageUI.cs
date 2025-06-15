using TMPro;
using UnityEngine;

public class PrivateMessageUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 3f;

    public static PrivateMessageUI Instance;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), displayTime);
    }

    private void Hide()
    {
        canvasGroup.alpha = 0;
    }
}
