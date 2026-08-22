using UnityEngine;
using System.Collections.Generic;

namespace OceanBattleRoyale.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private GameObject _playerShipPrefab;
        [SerializeField] private SpawnTest _spawnTest;
        [SerializeField] private float _matchDuration = 600f;

        [Header("State")]
        public float MatchTimeRemaining { get; private set; }
        public bool MatchStarted { get; private set; }
        public bool MatchEnded { get; private set; }

        private Dictionary<int, GameObject> _playerShips = new Dictionary<int, GameObject>();
        private List<int> _alivePlayerIds = new List<int>();
        private int _nextPlayerId = 1;

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

        private void Start()
        {
            MatchTimeRemaining = _matchDuration;
            MatchStarted = false;
            MatchEnded = false;

            StartMatch();
        }

        private void Update()
        {
            if (!MatchStarted || MatchEnded) return;

            MatchTimeRemaining -= Time.deltaTime;
            if (MatchTimeRemaining <= 0)
            {
                EndMatch();
            }
        }

        private void StartMatch()
        {
            MatchStarted = true;
            SpawnLocalPlayer();
        }

        private void SpawnLocalPlayer()
        {
            int playerId = _nextPlayerId++;
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject ship = Instantiate(_playerShipPrefab, spawnPos, Quaternion.identity);

            var networkedShip = ship.GetComponent<NetworkedShip>();
            if (networkedShip != null)
            {
                networkedShip.IsLocalPlayer = true;
            }

            var controller = ship.GetComponent<Network.LocalPlayerController>();
            if (controller == null)
            {
                ship.AddComponent<Network.LocalPlayerController>();
            }

            _playerShips[playerId] = ship;
            _alivePlayerIds.Add(playerId);

            SetupLocalCamera(ship);
        }

        private void SetupLocalCamera(GameObject ship)
        {
            if (Camera.main == null) return;
            Camera.main.transform.SetParent(ship.transform);
            Camera.main.transform.localPosition = new Vector3(0, 20, -30);
            Camera.main.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 200f;
            return new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        public void OnShipDied(GameObject ship)
        {
            int foundId = -1;
            foreach (var kvp in _playerShips)
            {
                if (kvp.Value == ship)
                {
                    foundId = kvp.Key;
                    break;
                }
            }

            if (foundId >= 0)
            {
                _playerShips.Remove(foundId);
                _alivePlayerIds.Remove(foundId);

                bool wasLocal = ship.GetComponent<NetworkedShip>() != null &&
                                ship.GetComponent<NetworkedShip>().IsLocalPlayer;

                if (wasLocal)
                {
                    StartCoroutine(RespawnLocalPlayer());
                }

                CheckMatchEnd();
            }
        }

        private System.Collections.IEnumerator RespawnLocalPlayer()
        {
            yield return new WaitForSeconds(5f);
            if (!MatchEnded)
            {
                SpawnLocalPlayer();
            }
        }

        private void CheckMatchEnd()
        {
            if (_alivePlayerIds.Count <= 1)
            {
                EndMatch();
            }
        }

        private void EndMatch()
        {
            MatchEnded = true;
            MatchTimeRemaining = 0;
            Debug.Log("[GameManager] Match ended.");
        }
    }
}
