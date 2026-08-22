using UnityEngine;

namespace OceanBattleRoyale.Core
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Settings")]
        [SerializeField] private string _gameSceneName = "Prototype";

        [Header("Connection")]
        [SerializeField] private int _maxPlayers = 50;
        [SerializeField] private string _sessionName = "OceanBattle";

        public static NetworkManager Instance { get; private set; }
        public bool IsConnected => false;
        public bool IsServer => true;
        public bool IsHost => true;

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

        public void StartGame()
        {
            Debug.Log("[NetworkManager] Starting local game");
        }

        public void Shutdown()
        {
            Debug.Log("[NetworkManager] Shutdown");
        }
    }
}
