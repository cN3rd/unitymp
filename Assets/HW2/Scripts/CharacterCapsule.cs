using Fusion;
using UnityEngine;

namespace HW2.Scripts
{
    public class CharacterCapsule : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [Networked]
        [OnChangedRender(nameof(OnColorChanged))]
        public Color Color { get; set; }

        public void OnColorChanged() => meshRenderer.material.color = Color;
        
        public override void Spawned()
        {
            NetworkObject parentObject = Runner.GetPlayerObject(Object.InputAuthority);
            transform.SetParent(parentObject.transform);
            
            OnColorChanged();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!meshRenderer)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }
        #endif
    }
}
