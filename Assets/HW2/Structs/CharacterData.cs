using UnityEngine;
using Fusion;

namespace HW2.Scripts
{
    public class CharacterData : INetworkStruct
    {
        public Color Color;
        public string Name;

        public CharacterData(string name = default, Color color = default)
        {
            Color = color;
            Name = name;
        }
    }
}
