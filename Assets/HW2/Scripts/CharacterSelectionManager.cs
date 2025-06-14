using UnityEngine;
using System.Collections.Generic;
using Fusion;
using UnityEngine.SceneManagement;

namespace HW2.Scripts
{
    public class CharacterSelectionManager : NetworkBehaviour
    {
        [Header("HUD Stuff")] [SerializeField] List<CharacterButton> selectButtons;
        [SerializeField] RectTransform charcterButtonParent;

        [Header("Player Stuff")] [SerializeField]
        CharacterCapsule playerCapsulePrefab;

        private readonly HashSet<Color> _takenCharactersColor = new();
        private NetworkRunner _runnerSession;

        private readonly Vector3[] _positions = { new(-2, 0, 0), new(0, 0, 2), new(2, 0, -2) }; // Temporery for debugging

        private void OnEnable()
        {
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked += (Color selectedColor) =>
                {
                    Vector3 randomSpawn = _positions[Random.Range(0, _positions.Length)];
                    Rpc_RequestCharacterSelection(selectedColor, randomSpawn);
                };
            }
        }

        private void Start() =>
            _runnerSession = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
        public void Rpc_RequestCharacterSelection(Color characterColor, Vector3 spawnPosition,
            RpcInfo info = default)
        {
            if (_takenCharactersColor.Add(characterColor))
            {
                // Adding the color if there isnt same color there
                // Approved
                RPC_CharacterApproved(info, characterColor, spawnPosition);
            }
            else
            {
                // Deny
                RPC_CharacterDenied(info); // Color already exits so you cant take that character
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_CharacterApproved(RpcInfo info, Color characterColor, Vector3 spawnPosition)
        {
            //if (_runnerSession.LocalPlayer != info.Source) return;
            if (!this.HasStateAuthority) return; // Only the host does the spawning


            Debug.Log($"Character {characterColor} approved. Spawning...");

            // Spawn Logic :
            CharacterCapsule playerToSpawn = _runnerSession.Spawn(playerCapsulePrefab);
            playerToSpawn.MyColor = characterColor;
            //SpawnPlayer(characterColor);
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_CharacterDenied(RpcInfo info)
        {
            if (!this.HasStateAuthority) return; // Only the host does the spawning

            Debug.Log("Character already taken. Choose another.");

            // Trigger UI update or retry - Pop up already taken 
        }


        private void SpawnPlayer(Color color)
        {
            CharacterCapsule playerToSpawn = _runnerSession.Spawn(playerCapsulePrefab);
            playerToSpawn.MyColor = color;
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
}
