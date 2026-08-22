using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace OceanBattleRoyale.Core
{
    public class GameManager : NetworkBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private NetworkedShip _playerShipPrefab;
        [SerializeField] private SpawnTest _spawnTest;
        [SerializeField] private float _matchDuration = 600f; // 10 minutes

        [Header("State")]
        [Networked] public float MatchTimeRemaining { get; set; }
        [Networked] public NetworkBool MatchStarted { get; set; }
        [Networked] public NetworkBool MatchEnded { get; set; }

        private Dictionary<PlayerRef, NetworkedShip> _playerShips = new Dictionary<PlayerRef, NetworkedShip>();

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                MatchTimeRemaining = _matchDuration;
                MatchStarted = false;
                MatchEnded = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            if (!MatchStarted && Runner.ActivePlayers.Count > 0)
            {
                MatchStarted = true;
                SpawnAllPlayers();
            }

            if (MatchStarted && !MatchEnded)
            {
                MatchTimeRemaining -= Runner.DeltaTime;
                if (MatchTimeRemaining <= 0)
                {
                    EndMatch();
                }
            }
        }

        private void SpawnAllPlayers()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                SpawnPlayerShip(player);
            }
        }

        private void SpawnPlayerShip(PlayerRef player)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            var ship = Runner.Spawn(_playerShipPrefab, spawnPos, Quaternion.identity, player);
            _playerShips[player] = ship;

            if (player == Runner.LocalPlayer)
            {
                // Setup local player camera, input, etc.
                SetupLocalPlayer(ship);
            }
        }

        private void SetupLocalPlayer(NetworkedShip ship)
        {
            Camera.main.transform.SetParent(ship.transform);
            Camera.main.transform.localPosition = new Vector3(0, 20, -30);
            Camera.main.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 200f;
            return new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        public void OnPlayerDied(PlayerRef player, PlayerRef killer)
        {
            if (!Object.HasStateAuthority) return;

            if (_playerShips.TryGetValue(player, out var ship))
            {
                Runner.Despawn(ship.Object);
                _playerShips.Remove(player);

                // Respawn after delay
                StartCoroutine(RespawnPlayer(player));
            }

            CheckMatchEnd();
        }

        private System.Collections.IEnumerator RespawnPlayer(PlayerRef player)
        {
            yield return new WaitForSeconds(5f);

            if (Runner.IsRunning && Runner.ActivePlayers.Contains(player))
            {
                SpawnPlayerShip(player);
            }
        }

        private void CheckMatchEnd()
        {
            int aliveCount = 0;
            PlayerRef lastPlayer = PlayerRef.None;

            foreach (var kvp in _playerShips)
            {
                if (kvp.Value.IsAlive)
                {
                    aliveCount++;
                    lastPlayer = kvp.Key;
                }
            }

            if (aliveCount <= 1)
            {
                EndMatch(lastPlayer);
            }
        }

        private void EndMatch(PlayerRef winner = default)
        {
            MatchEnded = true;
            MatchTimeRemaining = 0;

            // Show results, award XP, etc.
            Debug.Log($"[GameManager] Match ended. Winner: {winner}");
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _playerShips.Clear();
        }
    }
}
