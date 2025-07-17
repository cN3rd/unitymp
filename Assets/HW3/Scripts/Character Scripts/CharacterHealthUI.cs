using UnityEngine;
using UnityEngine.UI;

namespace HW3.Scripts
{
    public class CharacterHealthUI : MonoBehaviour
    {
        [SerializeField] private CharacterHealth healthComponent;
        [SerializeField] private Slider healthSlider;

        private void Awake() => healthComponent.OnHealthChanged += HealthChanged;

        private void HealthChanged(float newFraction) => healthSlider.value = newFraction;
    }
}
