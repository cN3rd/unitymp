using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fusion;
using HW1.Scripts;
public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("HUD Stuff")]
    [SerializeField] List<CharacterButton> selectButtons;
    [SerializeField] RectTransform charcterButtonParent;

    [Header("Player Stuff")]
    [SerializeField] CharacterCapsule playerCapsulePrefab;

    private HashSet<Color> takenCharactersColor = new HashSet<Color>();

    private void OnEnable()
    {
        foreach (CharacterButton button in selectButtons)
        {
            button.OnButtonClicked += SpawnPlayer;
        }
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
        NetworkRunner networkRunner = LobbyManager.Instance.SessionRunnerInstance;

        CharacterCapsule playerToSpawn = networkRunner.Spawn(playerCapsulePrefab);
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
