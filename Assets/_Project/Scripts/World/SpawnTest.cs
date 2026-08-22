using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace OceanBattleRoyale.World
{
    public class SpawnTest : NetworkBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private NetworkedShip _shipPrefab;
        [SerializeField] private int _botCount = 50;
        [SerializeField] private float _spawnRadius = 500f;
        [SerializeField] private float _safeZoneRadius = 50f;

        [Header("Interest Management")]
        [SerializeField] private float _interestRadius = 100f;
        [SerializeField] private float _viewRadius = 200f;

        private List<NetworkedShip> _spawnedShips = new List<NetworkedShip>();
        private bool _hasSpawned = false;

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                SetupInterestManagement();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority && !_hasSpawned && Runner.ActivePlayers.Count > 0)
            {
                SpawnBots();
                _hasSpawned = true;
            }
        }

        private void SetupInterestManagement()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                Runner.SetPlayerAreaOfInterest(player, Vector3.zero, _viewRadius);
            }
        }

        private void SpawnBots()
        {
            for (int i = 0; i < _botCount; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                var ship = Runner.Spawn(_shipPrefab, spawnPos, Quaternion.identity, PlayerRef.None);
                _spawnedShips.Add(ship);

                var botAI = ship.gameObject.AddComponent<BotAI>();
                botAI.Initialize(spawnPos, _spawnRadius);
            }

            Debug.Log($"[SpawnTest] Spawned {_botCount} bots");
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 pos;
            int attempts = 0;
            do
            {
                pos = UnityEngine.Random.insideUnitCircle * _spawnRadius;
                pos = new Vector3(pos.x, 0, pos.y);
                attempts++;
            } while (pos.magnitude < _safeZoneRadius && attempts < 10);

            return pos;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _spawnedShips.Clear();
        }
    }

    public class BotAI : MonoBehaviour
    {
        private NetworkedShip _ship;
        private ShipPhysics _physics;
        private Vector3 _targetPosition;
        private float _nextTargetTime;
        private float _spawnRadius;

        public void Initialize(Vector3 spawnPos, float spawnRadius)
        {
            _ship = GetComponent<NetworkedShip>();
            _physics = GetComponent<ShipPhysics>();
            _spawnRadius = spawnRadius;
            SetRandomTarget();
        }

        private void Update()
        {
            if (_ship == null || !_ship.IsAlive) return;

            if (Time.time > _nextTargetTime)
            {
                SetRandomTarget();
            }

            SteerTowardsTarget();
        }

        private void SetRandomTarget()
        {
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            float distance = UnityEngine.Random.Range(_spawnRadius * 0.3f, _spawnRadius * 0.8f);
            _targetPosition = new Vector3(randomDir.x * distance, 0, randomDir.y * distance);
            _nextTargetTime = Time.time + UnityEngine.Random.Range(5f, 15f);
        }

        private void SteerTowardsTarget()
        {
            Vector3 toTarget = _targetPosition - transform.position;
            toTarget.y = 0;
            float distance = toTarget.magnitude;

            if (distance < 10f)
            {
                SetRandomTarget();
                return;
            }

            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(forward, toTarget.normalized, Vector3.up);

            var input = new OceanBattleRoyale.Network.ShipInput
            {
                Move = new Vector2(Mathf.Clamp(angle / 30f, -1f, 1f), 1f),
                Aim = Vector2.zero,
                Fire = false,
                DeployMine = false,
                WeaponSwitch = 0
            };

            _physics.Simulate(input, Time.deltaTime);
        }
    }
}
