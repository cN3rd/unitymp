using Fusion;
using UnityEngine;

namespace HW2.Scripts
{
    public class CharacterCapsule : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [Networked]
        [OnChangedRender(nameof(OnColorChanged))]
        public Color MeshColor { get; set; }

        [Networked]
        [OnChangedRender(nameof(OnNameChanged))]
        public NetworkString<_32> PlayerName { get; set; } // Idan You Can Take This For Your Chat 

        private void OnValidate()
        {
            if (!meshRenderer)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }

        public void OnColorChanged() => meshRenderer.material.color = MeshColor;

        public void OnNameChanged() => gameObject.name = PlayerName.ToString();

        public override void Spawned()
        {
            OnColorChanged();
            OnNameChanged();
        }
    }
}
