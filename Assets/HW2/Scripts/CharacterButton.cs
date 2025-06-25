using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HW2.Scripts
{
    public class CharacterButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image characterImage; // Can be actual character image
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Color color;
        
        public Color ButtonCharacterColor => characterImage.color;

        private void OnEnable()
        {
            Debug.Log($"Enabled button \"{button.gameObject.name}\"");
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(CharacterSelected);
        }

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

            if (characterImage.color != color)
            {
                color.a = 1;
                characterImage.color = color;
            }

            if (characterImage.color.a < 1)
            {
                Color color = characterImage.color;
                color.a = 1;

                characterImage.color = color;
            }

            #endregion
        }

        public event UnityAction<Color, Transform> OnButtonClicked;

        public void SetColor(Color newColor) => characterImage.color = newColor;

        public void CharacterSelected()
        {
            Debug.Log($"You Clicked On Me {ButtonCharacterColor}");
            OnButtonClicked?.Invoke(characterImage.color, spawnPoint);
        }
    }
}
