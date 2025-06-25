using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW2.Scripts
{
    public class CharacterSelectionManager : NetworkBehaviour
    {
        [Header("HUD Stuff")]
        [SerializeField] private List<CharacterButton> selectButtons;
        [SerializeField] private RectTransform charcterButtonParent;
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private GameObject characterSelectionUI;

        [Header("Player Stuff")]
        [SerializeField] private CharacterCapsule playerCapsulePrefab;
        
        private readonly HashSet<string> _takenCharactersColor = new();
        private bool _hasSelectedCharacter;
        private NetworkRunner _runnerSession;

        private void Start() =>
            _runnerSession = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());

        private void OnDisable()
        {
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked -= OnButtonOnOnButtonClicked;
            }
        }

        private void OnValidate()
        {
            if (selectButtons.Count <= 0 && charcterButtonParent)
            {
                selectButtons.Clear(); // Clear first to avoid duplicates
                foreach (RectTransform child in charcterButtonParent)
                {
                    CharacterButton characterButton = child.GetComponent<CharacterButton>();
                    if (characterButton != null) // Add null check
                    {
                        selectButtons.Add(characterButton);
                    }
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked += OnButtonOnOnButtonClicked;
            }
        }

        private void OnButtonOnOnButtonClicked(Color selectedColor, Transform spawnPointPosition)
        {
            // Prevent multiple selections
            if (_hasSelectedCharacter)
            {
                errorPopup?.ShowError("You have already selected a character.");
                return;
            }

            Debug.Log("Button clicked");
            Rpc_RequestCharacterSelection(selectedColor, spawnPointPosition.transform.position);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RequestCharacterSelection(Color characterColor, Vector3 spawnPosition,
            RpcInfo info = default)
        {
            string colorString = ColorToString(characterColor);
            Debug.Log($"Checking Color {colorString} if available for player {info.Source}");

            // Only state authority should manage character selection
            if (!HasStateAuthority)
            {
                Debug.LogError("Only state authority can handle character selection!");
                return;
            }

            if (_takenCharactersColor.Add(colorString))
            {
                // Adding the color if there isn't same color there
                // Approved - Pass the actual requesting player as parameter
                Debug.Log($"Character {colorString} approved for player {info.Source}");
                RPC_CharacterApproved(info.Source, characterColor,
                    spawnPosition); // Pass info.Source as parameter
            }
            else
            {
                // Deny
                Debug.Log($"Character {colorString} denied - already taken");
                RPC_CharacterDenied(info
                    .Source); // Color already exists so you can't take that character
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CharacterApproved([RpcTarget] PlayerRef targetPlayer, Color characterColor,
            Vector3 spawnPosition)
        {
            SpawnPlayerCharacter(targetPlayer, characterColor, spawnPosition);
            
            _hasSelectedCharacter = true;
            characterSelectionUI?.SetActive(false);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CharacterDenied([RpcTarget] PlayerRef targetPlayer)
        {
            if (!characterSelectionUI.gameObject
                    .activeSelf) // A way to skip this function when its called multiple times
                return;

            errorPopup?.ShowError("Character already taken. Choose another");
            // Trigger UI update or retry - Pop up already taken 
        }

        private void SpawnPlayerCharacter(PlayerRef playerRef, Color characterColor,
            Vector3 spawnPosition)
        {
            CharacterCapsule playerToSpawn = _runnerSession.Spawn(
                playerCapsulePrefab,
                spawnPosition,
                Quaternion.identity,
                playerRef
            );

            if (playerToSpawn == null)
            {
                Debug.LogError("Failed to spawn player character!");
                return;
            }
            
            playerToSpawn.Color = characterColor;
        }

        private string ColorToString(Color color) =>
            $"{color.r:F3},{color.g:F3},{color.b:F3},{color.a:F3}";
    }
}
