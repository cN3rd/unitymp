using Fusion;
using UnityEngine;

public class CharacterCapsule : NetworkBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;

    public void SetColor(Color color = default)
    {
        meshRenderer.material.color = color;
    }


    private void OnValidate()
    {
        if (!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
