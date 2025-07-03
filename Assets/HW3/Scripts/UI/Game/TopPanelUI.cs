using System;
using UnityEngine;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class TopPanelUI : BasePanel
    {
        [SerializeField] private Button dmButton;
        [SerializeField] private Button closeSessionButton;

        public event Action OnCloseSessionRequested;
        public event Action OnDirectMessageRequested;

        private void Awake()
        {
            closeSessionButton.onClick.AddListener(() => OnCloseSessionRequested?.Invoke());
            dmButton.onClick.AddListener(() => OnDirectMessageRequested?.Invoke());
        }

        public void ShowTopPanel(bool isMasterClient)
        {
            closeSessionButton.gameObject.SetActive(isMasterClient);
            ShowPopup();
        }
    }
}
