using Fusion;
using UnityEngine;

public class CharacterCapsule : NetworkBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;

    //public void SetColor(Color color = default)
    //{
    //    int colorsLength = meshFilter.mesh.colors.Length;

    //    for (int i = 0; i < colorsLength; i++)
    //    {
    //        meshFilter.mesh.colors[i] = color;
    //    }
    //}
    public void SetColor(Color color = default)
    {
        meshRenderer.material.color = color;
    }


    private void OnValidate()
    {
        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    }
}
