using UnityEngine;
using System.Collections.Generic;
using Fusion;
using UnityEngine.SceneManagement;

namespace HW2.Scripts
{
    public class CharacterSelectionManager : NetworkBehaviour
    {
        [Header("HUD Stuff")]
        [SerializeField] List<CharacterButton> selectButtons;
        [SerializeField] RectTransform charcterButtonParent;
        [SerializeField] ErrorPopup errorPopup;
        [SerializeField] GameObject characterSelectionUI; // Reference to the UI panel instead of this GameObject

        [Header("Player Stuff")]
        [SerializeField] CharacterCapsule playerCapsulePrefab;

        private readonly HashSet<string> _takenCharactersColor = new();
        private NetworkRunner _runnerSession;
        private bool _hasSelectedCharacter = false;

        private void OnEnable()
        {
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked += (Color selectedColor, Transform spawnPointPosition) =>
                {
                    // Prevent multiple selections
                    if (_hasSelectedCharacter)
                    {
                        errorPopup?.ShowError("You have already selected a character.");
                        return;
                    }

                    Rpc_RequestCharacterSelection(selectedColor, spawnPointPosition.transform.position);
                };
            }
        }

        private void OnDisable()
        {
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked -= (Color selectedColor, Transform spawnPointPosition) =>
                {
                    Rpc_RequestCharacterSelection(selectedColor, spawnPointPosition.transform.position);
                };
            }
        }

        private void Start() =>
            _runnerSession = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
        public void Rpc_RequestCharacterSelection(Color characterColor, Vector3 spawnPosition, RpcInfo info = default)
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
                RPC_CharacterApproved(info.Source, characterColor, spawnPosition); // Pass info.Source as parameter
            }
            else
            {
                // Deny
                Debug.Log($"Character {colorString} denied - already taken");
                RPC_CharacterDenied(info.Source); // Color already exists so you can't take that character
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_CharacterApproved(PlayerRef originalPlayer, Color characterColor, Vector3 spawnPosition)
        {
            // Only the state authority spawns the player
            if (HasStateAuthority)
            {
                SpawnPlayerCharacter(originalPlayer, characterColor, spawnPosition);
            }


            // Only hide UI for the player who made the selection
            if (_runnerSession.LocalPlayer == originalPlayer)
            {

                // Mark as selected to prevent future selections
                _hasSelectedCharacter = true;

                // Hide selection UI for this player only
                if (characterSelectionUI != null)
                {
                    characterSelectionUI.SetActive(false);
                }
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_CharacterDenied(PlayerRef targetPlayer)
        {
            if (_runnerSession.LocalPlayer != targetPlayer)
                return;

            if (!characterSelectionUI.gameObject.activeSelf) // A way to skip this function when its called multiple times
                return;

            errorPopup?.ShowError("Character already taken. Choose another");
            // Trigger UI update or retry - Pop up already taken 
        }

        private void SpawnPlayerCharacter(PlayerRef playerRef, Color characterColor, Vector3 spawnPosition)
        {
            // Spawn Logic - Set Player Data
            CharacterCapsule playerToSpawn = _runnerSession.Spawn(
                playerCapsulePrefab,
                spawnPosition,
                Quaternion.identity,
                playerRef // Assign input authority to the correct player
            );

            if (playerToSpawn == null)
            {
                Debug.LogError("Failed to spawn player character!");
                return;
            }

            // Set player name
            if (PlayerNameContainer.Instance && PlayerNameContainer.Instance.PlayersDictionary.TryGet(playerRef, out var playerName))
            {
                playerToSpawn.PlayerName = playerName;
            }
            else
            {
                playerToSpawn.PlayerName = playerRef.PlayerId.ToString();
            }

            playerToSpawn.MeshColor = characterColor;
        }

        private string ColorToString(Color color)
        {
            return $"{color.r:F3},{color.g:F3},{color.b:F3},{color.a:F3}"; // Easier to Read
        }

       
        private void SetButtonsInteractable(bool interactable)
        {
            foreach (CharacterButton button in selectButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    button.gameObject.SetActive(interactable);
                }
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
    }
}
