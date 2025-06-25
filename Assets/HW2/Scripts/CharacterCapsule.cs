using Fusion;
using System;
using UnityEngine;
using HW2;
using HW2.Scripts;

public class CharacterCapsule : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnColorChanged))]
    public Color MeshColor { get; set; }

    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public NetworkString<_32> PlayerName { get; set; } // Idan You Can Take This For Your Chat 
    [SerializeField] MeshRenderer meshRenderer;

    public void OnColorChanged()
    {
        meshRenderer.material.color = MeshColor;
    }

    public void OnNameChanged()
    {
        gameObject.name = PlayerName.ToString();
    }

    public override void Spawned()
    {
        OnColorChanged();
        OnNameChanged();
    }

    private void OnValidate()
    {
        if (!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
