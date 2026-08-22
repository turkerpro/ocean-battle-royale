using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private bool _isInLobby = false;
        private string _currentSessionName = "";

        public static LobbyManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
            if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
            if (_loadingPanel != null) _loadingPanel.SetActive(false);

            if (_createLobbyButton != null) _createLobbyButton.onClick.AddListener(CreateLobby);
            if (_joinLobbyButton != null) _joinLobbyButton.onClick.AddListener(JoinLobby);
            if (_quickMatchButton != null) _quickMatchButton.onClick.AddListener(QuickMatch);
            if (_startGameButton != null) _startGameButton.onClick.AddListener(StartGame);
            if (_leaveLobbyButton != null) _leaveLobbyButton.onClick.AddListener(LeaveLobby);
            if (_maxPlayersSlider != null) _maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersChanged);

            if (_sessionNameInput != null) _sessionNameInput.text = "OceanBattle_" + Random.Range(1000, 9999);
            if (_playerNameInput != null) _playerNameInput.text = "Captain_" + Random.Range(100, 999);
        }

        private void OnMaxPlayersChanged(float value)
        {
            if (_maxPlayersText != null) _maxPlayersText.text = "Max Players: " + Mathf.RoundToInt(value);
        }

        private void CreateLobby()
        {
            string sessionName = _sessionNameInput != null ? _sessionNameInput.text.Trim() : "Lobby";
            if (string.IsNullOrEmpty(sessionName)) return;

            SetLoading(true, "Creating lobby...");
            _isInLobby = true;
            _currentSessionName = sessionName;
            ShowLobbyPanel();
        }

        private void JoinLobby()
        {
            string sessionName = _sessionNameInput != null ? _sessionNameInput.text.Trim() : "Lobby";
            if (string.IsNullOrEmpty(sessionName)) return;

            SetLoading(true, "Joining lobby...");
            _isInLobby = true;
            _currentSessionName = sessionName;
            ShowLobbyPanel();
        }

        private void QuickMatch()
        {
            SetLoading(true, "Finding match...");
            _isInLobby = true;
            ShowLobbyPanel();
        }

        private void StartGame()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.StartGame();
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("Prototype");
        }

        private void LeaveLobby()
        {
            _isInLobby = false;
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
            if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
        }

        private void ShowLobbyPanel()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_lobbyPanel != null) _lobbyPanel.SetActive(true);
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
            UpdateLobbyUI();
        }

        private void SetLoading(bool active, string message = "")
        {
            if (_loadingPanel != null) _loadingPanel.SetActive(active);
            if (_loadingPanel != null)
            {
                var text = _loadingPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = message;
            }
        }

        private void UpdateLobbyUI()
        {
            if (_lobbyNameText != null) _lobbyNameText.text = _currentSessionName;
        }
    }
}
