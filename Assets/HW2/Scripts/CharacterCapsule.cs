using Fusion;
using System;
using UnityEngine;

public class CharacterCapsule : NetworkBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;

    [Networked, OnChangedRender(nameof(SetColor))] public Color MyColor {  get; set; }

    public override void Spawned()
    {
        // Apply color when object is first seen
        SetColor();
    }

    public void SetColor()
    {
        //myColor = color;
        meshRenderer.material.color = MyColor;
        Debug.Log($"Colored Change {MyColor}");
    }


    private void OnValidate()
    {
        if (!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}

[System.Serializable]
public struct CharacterData
{
    Color MeshColor;
    string Name;
}
