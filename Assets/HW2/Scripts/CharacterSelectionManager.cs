using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fusion;
using HW2.Scripts;
public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("HUD Stuff")]
    [SerializeField] List<CharacterButton> selectButtons;
    [SerializeField] RectTransform charcterButtonParent;

    [Header("Player Stuff")]
    [SerializeField] CharacterCapsule playerCapsulePrefab;

    private HashSet<Color> takenCharactersColor = new HashSet<Color>();

    private NetworkRunner _runnerSession; // For Debugging - This object need to be spawned on load scene or conctted there
    private Vector3[] positions = new Vector3[] // Temporery positions that the Players can spawn. Need to be calculted like what Lior did at the class
    {
    new Vector3(-2, 0, 0),
    new Vector3(0, 0, 2),
    new Vector3(2, 0, -2)
    };

    private void OnEnable()
    {
        foreach (CharacterButton button in selectButtons)
        {
            button.OnButtonClicked += (Color selectedColor) =>
            {
                Vector3 randomSpawn = positions[Random.Range(0, positions.Length)];
                Rpc_RequestCharacterSelection(selectedColor, randomSpawn);
            };
        }
    }
    public void AssignNetwork(NetworkRunner runner)
    {
        _runnerSession = runner; 
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void Rpc_RequestCharacterSelection(Color characterColor, Vector3 spawnPosition, RpcInfo info = default)
    {
        if (!takenCharactersColor.Contains(characterColor))
        {
            takenCharactersColor.Add(characterColor); // Adding the color if there isnt same color there

            // Approve
            RPC_CharacterApproved(info, characterColor, spawnPosition);
        }
        else
        {
            // Deny
            RPC_CharacterDenied(info); // Color already exits so you cant take that character
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
    private void RPC_CharacterApproved(RpcInfo runner, Color characterColor, Vector3 spawnPosition)
    {
        Debug.Log($"Character {characterColor} approved. Spawning...");

        // Spawn Logic :
        SpawnPlayer(characterColor);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
    private void RPC_CharacterDenied(RpcInfo runner)
    {
        Debug.Log("Character already taken. Choose another.");

        // Trigger UI update or retry
    }

    private void SpawnPlayer(Color color)
    {
        CharacterCapsule playerToSpawn = _runnerSession.Spawn(playerCapsulePrefab);
        playerToSpawn.SetColor(color);
    }


    private void OnValidate()
    {
        if (selectButtons.Count <= 0 && charcterButtonParent)
        {
            foreach (RectTransform child in charcterButtonParent)
            {
                CharacterButton characterButton = child.GetComponent<CharacterButton>();
                selectButtons.Add(characterButton);
            }
        }
    }

}
