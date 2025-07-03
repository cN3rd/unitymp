using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class CharacterButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image characterImage;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Color color;
        
        private void Awake()
        {
            Debug.Log($"Enabled button \"{button.gameObject.name}\"");
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(CharacterSelected);
        }
        public event UnityAction<Color, Transform> OnButtonClicked;

        private void CharacterSelected()
        {
            Color characterColor = characterImage.color;
            Debug.Log($"You Clicked On Me {characterColor}");
            OnButtonClicked?.Invoke(characterColor, spawnPoint);
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            #region << Easy way to control the image colors
            if (!characterImage)
            {
                foreach (Transform child in transform)
                {
                    characterImage = child.GetComponent<Image>();
                    if (characterImage != null) break;
                }
            }

            if (characterImage.color != color || characterImage.color.a < 1)
            {
                characterImage.color = new Color(color.r, color.g, color.b, 1);
            }

            #endregion
        }
        #endif
    }
}
