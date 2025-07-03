using TMPro;
using UnityEngine;

namespace HW3.Scripts
{
    public class DirectMessageView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI template;
        [SerializeField] private RectTransform viewport;

        public void OnNewMessage(string sender, string message)
        {
            TextMeshProUGUI newMessageView = Instantiate(template, viewport);
            newMessageView.text = $"<b>{sender}:</b> {message}";
            newMessageView.gameObject.SetActive(true);
            Destroy(newMessageView.gameObject, 5f);
        }
    }
}
