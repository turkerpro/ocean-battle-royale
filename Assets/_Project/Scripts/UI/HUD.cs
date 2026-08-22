using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OceanBattleRoyale.UI
{
    public class HUD : MonoBehaviour
    {
        [Header("Level & XP")]
        [SerializeField] private Slider _levelProgressBar;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _xpText;

        [Header("Health")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _healthFill;

        [Header("Weapons")]
        [SerializeField] private WeaponSlotUI[] _weaponSlots;
        [SerializeField] private TextMeshProUGUI _ammoText;

        [Header("Minimap")]
        [SerializeField] private RawImage _minimapImage;
        [SerializeField] private float _minimapSize = 200f;
        [SerializeField] private LayerMask _minimapLayers;

        [Header("Kill Feed")]
        [SerializeField] private Transform _killFeedContainer;
        [SerializeField] private GameObject _killFeedEntryPrefab;
        [SerializeField] private int _maxKillFeedEntries = 5;

        private NetworkedShip _localShip;
        private Camera _minimapCamera;
        private RenderTexture _minimapRT;

        public static HUD Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        private void Start()
        {
            SetupMinimap();
        }

        private void SetupMinimap()
        {
            if (_minimapImage == null) return;

            _minimapRT = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            _minimapImage.texture = _minimapRT;

            var camGO = new GameObject("MinimapCamera");
            _minimapCamera = camGO.AddComponent<Camera>();
            _minimapCamera.orthographic = true;
            _minimapCamera.orthographicSize = _minimapSize;
            _minimapCamera.targetTexture = _minimapRT;
            _minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            _minimapCamera.backgroundColor = new Color(0, 0, 0, 0);
            _minimapCamera.cullingMask = _minimapLayers;
            _minimapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        private void LateUpdate()
        {
            if (_localShip == null)
            {
                FindLocalShip();
                return;
            }

            UpdateLevelXP();
            UpdateHealth();
            UpdateMinimap();
        }

        private void FindLocalShip()
        {
            var ships = FindObjectsOfType<NetworkedShip>();
            foreach (var ship in ships)
            {
                if (ship.IsLocalPlayer)
                {
                    _localShip = ship;
                    gameObject.SetActive(true);
                    break;
                }
            }
        }

        private void UpdateLevelXP()
        {
            if (_localShip == null) return;

            int level = _localShip.CurrentLevel;
            int xp = _localShip.CurrentXP;
            int xpForNext = level * 100;

            if (_levelText) _levelText.text = "SEVIYE " + level;
            if (_xpText) _xpText.text = xp + " / " + xpForNext + " XP";
            if (_levelProgressBar) _levelProgressBar.value = (float)xp / xpForNext;
        }

        private void UpdateHealth()
        {
            if (_localShip == null) return;

            float health = _localShip.Health;
            float maxHealth = _localShip.MaxHealth;

            if (_healthBar) _healthBar.value = health / maxHealth;
            if (_healthText) _healthText.text = Mathf.CeilToInt(health) + " / " + Mathf.CeilToInt(maxHealth);

            if (_healthFill)
            {
                float ratio = health / maxHealth;
                _healthFill.color = Color.Lerp(Color.red, Color.green, ratio);
            }
        }

        private void UpdateMinimap()
        {
            if (_minimapCamera == null || _localShip == null) return;

            _minimapCamera.transform.position = new Vector3(
                _localShip.transform.position.x,
                _minimapSize * 2,
                _localShip.transform.position.z
            );
        }

        public void AddKillFeed(string killer, string victim, bool isLocalPlayer)
        {
            if (_killFeedContainer == null || _killFeedEntryPrefab == null) return;

            var entry = Instantiate(_killFeedEntryPrefab, _killFeedContainer);
            entry.transform.SetAsFirstSibling();

            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = killer;
                texts[1].text = victim;
                texts[0].color = isLocalPlayer ? Color.green : Color.white;
                texts[1].color = Color.red;
            }

            while (_killFeedContainer.childCount > _maxKillFeedEntries)
            {
                Destroy(_killFeedContainer.GetChild(_killFeedContainer.childCount - 1).gameObject);
            }

            Destroy(entry, 5f);
        }

        public void SetWeaponSlot(int index, Sprite icon, string name, int ammo, bool isActive)
        {
            if (index < 0 || index >= _weaponSlots.Length) return;
            _weaponSlots[index].Setup(icon, name, ammo, isActive);
        }

        private void OnDestroy()
        {
            if (_minimapRT != null) _minimapRT.Release();
        }
    }

    [System.Serializable]
    public class WeaponSlotUI
    {
        public Image Icon;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI AmmoText;
        public GameObject ActiveIndicator;

        public void Setup(Sprite icon, string name, int ammo, bool isActive)
        {
            if (Icon) Icon.sprite = icon;
            if (NameText) NameText.text = name;
            if (AmmoText) AmmoText.text = ammo >= 0 ? ammo.ToString() : "\u221E";
            if (ActiveIndicator) ActiveIndicator.SetActive(isActive);
        }
    }
}
