using UnityEngine;

namespace HW2.Scripts
{
    public class BasePanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        private void Start() => HidePopup();

        protected void ShowPopup()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }

        protected void HidePopup()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }
    }
}
