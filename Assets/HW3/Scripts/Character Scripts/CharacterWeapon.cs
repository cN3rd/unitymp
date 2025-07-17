using System;
using Fusion;
using UnityEngine;

namespace HW3.Scripts
{
    public class CharacterWeapon : NetworkBehaviour
    {
        [SerializeField] private CharacterInput input;
        [SerializeField] private GameObject projectile;
        [SerializeField] private Transform projectileSpawnPoint;

        private void Awake() => input.OnAttack += DoAttack;

        private void DoAttack(NetworkObject _) => Runner.Spawn(projectile,
            projectileSpawnPoint.position, projectileSpawnPoint.rotation);
    }
}
