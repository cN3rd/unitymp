using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HW2.Scripts
{
    public class PlayerListUI : MonoBehaviour
    {
        [SerializeField] private RectTransform contentView;
        [SerializeField] private TextMeshProUGUI templateObject;

        private readonly List<GameObject> _playerListItems = new();

        public void RefreshPlayerList(List<string> players)
        {
            ClearList();

            foreach (string player in players)
            {
                TextMeshProUGUI playerLabel = Instantiate(templateObject, contentView);
                playerLabel.gameObject.SetActive(true);
                playerLabel.text = player;
                _playerListItems.Add(playerLabel.gameObject);
            }
        }

        private void ClearList()
        {
            foreach (GameObject playerTextObject in _playerListItems)
            {
                Destroy(playerTextObject);
            }

            _playerListItems.Clear();
        }
    }
}
