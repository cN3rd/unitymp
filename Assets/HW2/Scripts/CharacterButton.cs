using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public event UnityAction<Color> OnButtonClicked;

    [SerializeField] Button button;
    [SerializeField] Image characterImage; // Can be actucal character image
    public Color ButtonCharacterColor => characterImage.color;

    [SerializeField] Color color;

    private void OnEnable()
    {
        button.onClick.AddListener(CharacterSelected);
    }

    public void SetColor(Color color)
    {
        characterImage.color = color;
    }

    public void CharacterSelected()
    {
        Debug.Log($"You Clicked On Me {ButtonCharacterColor}");
        OnButtonClicked?.Invoke(characterImage.color);
    }

    private void OnValidate()
    {
        #region << Easy way to control the image colors
        if (!characterImage)
        {
            foreach (Transform child in this.transform)
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
}
