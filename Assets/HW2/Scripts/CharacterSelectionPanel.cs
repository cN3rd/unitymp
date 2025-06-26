using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HW2.Scripts
{
    public class CharacterSelectionPanel : NetworkBehaviour
    {
        [Header("HUD Stuff")]
        [SerializeField] private List<CharacterButton> selectButtons;
        [SerializeField] private RectTransform charcterButtonParent;
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private GameObject characterSelectionUI;

        [Header("Player Stuff")]
        [SerializeField] private CharacterCapsule playerCapsulePrefab;

        private readonly HashSet<Color> _takenCharactersColor = new();
        private bool _hasSelectedCharacter;
        private NetworkRunner _runnerSession;

        public event UnityAction<bool> OnCharacterSelected;
        
        private void Start() =>
            _runnerSession = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());

        private void OnDisable()
        {
            foreach (CharacterButton button in selectButtons)
            {
                button.OnButtonClicked -= OnButtonOnOnButtonClicked;
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
        private void Rpc_RequestCharacterSelection(Color color, Vector3 spawnPosition,
            RpcInfo info = default)
        {
            if (!HasStateAuthority)
            {
                Debug.LogError("Only state authority can handle character selection!");
                return;
            }
            
            Debug.Log($"Checking Color {color} if available for player {info.Source}");
            if (_takenCharactersColor.Add(color))
            {
                // Approved - Pass the actual requesting player as parameter
                Debug.Log($"Character {color} approved for player {info.Source}");
                RPC_CharacterApproved(info.Source, color, spawnPosition);
            }
            else
            {
                // Denied - color is already taken
                Debug.Log($"Character {color} denied - already taken");
                RPC_CharacterDenied(info.Source);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CharacterApproved([RpcTarget] PlayerRef targetPlayer, Color characterColor,
            Vector3 spawnPosition)
        {
            SpawnPlayerCharacter(targetPlayer, characterColor, spawnPosition);
            
            _hasSelectedCharacter = true;
            characterSelectionUI?.SetActive(false);
            OnCharacterSelected?.Invoke(Runner.IsSharedModeMasterClient);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_CharacterDenied([RpcTarget] PlayerRef targetPlayer) 
            => errorPopup?.ShowError("Character already taken. Choose another");

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
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (selectButtons.Count > 0 || !charcterButtonParent) return;
            selectButtons.Clear();
            
            foreach (RectTransform child in charcterButtonParent)
            {
                CharacterButton characterButton = child.GetComponent<CharacterButton>();
                if (characterButton != null)
                {
                    selectButtons.Add(characterButton);
                }
            }
        }
        #endif
    }
}
