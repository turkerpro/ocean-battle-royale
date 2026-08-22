using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OceanBattleRoyale.Core
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Network Settings")]
        [SerializeField] private NetworkRunner _runnerPrefab;
        [SerializeField] private NetworkSceneManagerDefault _sceneManagerPrefab;
        [SerializeField] private string _gameSceneName = "Prototype";

        [Header("Connection")]
        [SerializeField] private GameMode _gameMode = GameMode.Shared;
        [SerializeField] private int _maxPlayers = 50;
        [SerializeField] private string _sessionName = "OceanBattle";

        private NetworkRunner _runner;
        private bool _isConnecting = false;

        public static NetworkManager Instance { get; private set; }
        public NetworkRunner Runner => _runner;
        public bool IsConnected => _runner != null && _runner.IsRunning;
        public bool IsServer => _runner != null && _runner.IsServer;
        public bool IsSharedMode => _runner != null && _runner.Topology == SimulationConfig.Topologies.Shared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> StartGame(bool asHost = true)
        {
            if (_isConnecting) return false;
            _isConnecting = true;

            try
            {
                _runner = Instantiate(_runnerPrefab);
                _runner.name = "NetworkRunner";
                _runner.AddCallbacks(this);

                var sceneManager = Instantiate(_sceneManagerPrefab);
                _runner.SceneManager = sceneManager;

                var startArgs = new StartGameArgs
                {
                    GameMode = _gameMode,
                    SessionName = _sessionName,
                    PlayerCount = _maxPlayers,
                    Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
                    SceneManager = sceneManager,
                    ObjectPool = _runner.GetComponent<NetworkObjectPoolDefault>(),
                };

                if (asHost)
                {
                    startArgs.GameMode = GameMode.Host;
                }

                var result = await _runner.StartGame(startArgs);
                if (result.Ok)
                {
                    Debug.Log($"[NetworkManager] Started as {(_runner.IsServer ? "Host" : "Client")} in {_gameMode}");
                    return true;
                }
                else
                {
                    Debug.LogError($"[NetworkManager] Failed to start: {result.ShutdownReason}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Exception: {e}");
                return false;
            }
            finally
            {
                _isConnecting = false;
            }
        }

        public async void Shutdown()
        {
            if (_runner != null && _runner.IsRunning)
            {
                await _runner.Shutdown();
                _runner = null;
            }
        }

        #region INetworkRunnerCallbacks

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkManager] Player joined: {player}");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkManager] Player left: {player}");
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"[NetworkManager] Shutdown: {shutdownReason}");
            _runner = null;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }

        #endregion
    }
}
