using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OceanBattleRoyale.Core
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private TMP_InputField _sessionNameInput;
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Button _createLobbyButton;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _quickMatchButton;
        [SerializeField] private Transform _sessionListContainer;
        [SerializeField] private GameObject _sessionEntryPrefab;
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private GameObject _playerEntryPrefab;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _leaveLobbyButton;
        [SerializeField] private Slider _maxPlayersSlider;
        [SerializeField] private TextMeshProUGUI _maxPlayersText;

        [Header("Settings")]
        [SerializeField] private NetworkRunner _runnerPrefab;
        [SerializeField] private NetworkSceneManagerDefault _sceneManagerPrefab;
        [SerializeField] private string _gameSceneName = "Prototype";

        private NetworkRunner _runner;
        private List<SessionInfo> _cachedSessions = new List<SessionInfo>();
        private bool _isInLobby = false;
        private string _currentSessionName = "";

        public static LobbyManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _mainMenuPanel.SetActive(true);
            _lobbyPanel.SetActive(false);
            _loadingPanel.SetActive(false);

            _createLobbyButton.onClick.AddListener(CreateLobby);
            _joinLobbyButton.onClick.AddListener(JoinLobby);
            _quickMatchButton.onClick.AddListener(QuickMatch);
            _startGameButton.onClick.AddListener(StartGame);
            _leaveLobbyButton.onClick.AddListener(LeaveLobby);
            _maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersChanged);

            _sessionNameInput.text = "OceanBattle_" + Random.Range(1000, 9999);
            _playerNameInput.text = "Captain_" + Random.Range(100, 999);
        }

        private void OnMaxPlayersChanged(float value)
        {
            _maxPlayersText.text = $"Max Players: {Mathf.RoundToInt(value)}";
        }

        private async void CreateLobby()
        {
            string sessionName = _sessionNameInput.text.Trim();
            if (string.IsNullOrEmpty(sessionName)) return;

            SetLoading(true, "Creating lobby...");

            _runner = Instantiate(_runnerPrefab);
            _runner.name = "NetworkRunner";
            var sceneManager = Instantiate(_sceneManagerPrefab);
            _runner.SceneManager = sceneManager;

            var args = new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = sessionName,
                PlayerCount = Mathf.RoundToInt(_maxPlayersSlider.value),
                Scene = SceneRef.FromIndex(GetGameSceneIndex()),
                SceneManager = sceneManager,
            };

            var result = await _runner.StartGame(args);

            if (result.Ok)
            {
                _isInLobby = true;
                _currentSessionName = sessionName;
                ShowLobbyPanel();
                UpdateLobbyUI();
            }
            else
            {
                SetLoading(false);
                Debug.LogError($"Failed to create lobby: {result.ShutdownReason}");
            }
        }

        private async void JoinLobby()
        {
            string sessionName = _sessionNameInput.text.Trim();
            if (string.IsNullOrEmpty(sessionName)) return;

            SetLoading(true, "Joining lobby...");

            _runner = Instantiate(_runnerPrefab);
            _runner.name = "NetworkRunner";
            var sceneManager = Instantiate(_sceneManagerPrefab);
            _runner.SceneManager = sceneManager;

            var args = new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
                SceneManager = sceneManager,
            };

            var result = await _runner.StartGame(args);

            if (result.Ok)
            {
                _isInLobby = true;
                _currentSessionName = sessionName;
                ShowLobbyPanel();
            }
            else
            {
                SetLoading(false);
                Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
            }
        }

        private async void QuickMatch()
        {
            SetLoading(true, "Finding match...");

            _runner = Instantiate(_runnerPrefab);
            _runner.name = "NetworkRunner";
            var sceneManager = Instantiate(_sceneManagerPrefab);
            _runner.SceneManager = sceneManager;

            var args = new StartGameArgs
            {
                GameMode = GameMode.Client,
                Scene = SceneRef.FromIndex(GetGameSceneIndex()),
                SceneManager = sceneManager,
            };

            var result = await _runner.StartGame(args);

            if (result.Ok)
            {
                _isInLobby = true;
                ShowLobbyPanel();
            }
            else
            {
                SetLoading(false);
                Debug.LogError($"Quick match failed: {result.ShutdownReason}");
            }
        }

        private void StartGame()
        {
            if (_runner != null && _runner.IsRunning && _runner.IsServer)
            {
                _runner.SetActiveScene(_gameSceneName);
            }
        }

        private void LeaveLobby()
        {
            if (_runner != null && _runner.IsRunning)
            {
                _runner.Shutdown();
            }
            _isInLobby = false;
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            _mainMenuPanel.SetActive(true);
            _lobbyPanel.SetActive(false);
            _loadingPanel.SetActive(false);
        }

        private void ShowLobbyPanel()
        {
            _mainMenuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            _loadingPanel.SetActive(false);
            UpdateLobbyUI();
        }

        private void SetLoading(bool active, string message = "")
        {
            _loadingPanel.SetActive(active);
            var text = _loadingPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = message;
        }

        private void UpdateLobbyUI()
        {
            if (_lobbyNameText) _lobbyNameText.text = _currentSessionName;

            if (_playerListContainer && _runner != null)
            {
                foreach (Transform child in _playerListContainer) Destroy(child.gameObject);

                foreach (var player in _runner.ActivePlayers)
                {
                    var entry = Instantiate(_playerEntryPrefab, _playerListContainer);
                    var tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp) tmp.text = $"Player {player.PlayerId} {(player == _runner.LocalPlayer ? "(You)" : "")}";
                }
            }

            if (_startGameButton) _startGameButton.interactable = _runner != null && _runner.IsServer;
        }

        private int GetGameSceneIndex()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path;
                if (path.Contains(_gameSceneName)) return i;
            }
            return 1;
        }

        public void OnPlayerJoined(PlayerRef player)
        {
            UpdateLobbyUI();
        }

        public void OnPlayerLeft(PlayerRef player)
        {
            UpdateLobbyUI();
        }
    }
}
