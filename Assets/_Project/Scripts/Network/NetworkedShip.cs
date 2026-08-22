using UnityEngine;

namespace OceanBattleRoyale.Network
{
    public struct ShipInput
    {
        public Vector2 Move;
        public Vector2 Aim;
        public bool Fire;
        public bool DeployMine;
        public byte WeaponSwitch;
    }

    public class NetworkedShip : MonoBehaviour
    {
        public int CurrentTier = 1;
        public int CurrentLevel = 1;
        public int CurrentXP = 0;
        public float Health = 100f;
        public float MaxHealth = 100f;
        public byte ActiveWeaponSlot = 0;
        public bool IsAlive = true;
        public bool IsLocalPlayer = false;

        [SerializeField] private Renderer _hullRenderer;
        [SerializeField] private Renderer _turretRenderer;
        [SerializeField] private TrailRenderer _wakeTrail;

        private ShipPhysics _physics;
        private MaterialPropertyBlock _hullProps;
        private MaterialPropertyBlock _turretProps;

        private void Awake()
        {
            _physics = GetComponent<ShipPhysics>();
            _hullProps = new MaterialPropertyBlock();
            _turretProps = new MaterialPropertyBlock();
        }

        private void Start()
        {
            if (!IsLocalPlayer)
            {
                CurrentTier = UnityEngine.Random.Range(1, 4);
                CurrentLevel = UnityEngine.Random.Range(1, 20);
                MaxHealth = 100f * (1f + (CurrentLevel - 1) * 0.1f) * GetTierHealthMultiplier(CurrentTier);
                Health = MaxHealth;
            }
            IsAlive = true;
            UpdateVisuals();
        }

        public void Simulate(ShipInput input, float deltaTime)
        {
            if (_physics != null)
                _physics.Simulate(input, deltaTime);
        }

        public void TakeDamage(float damage, NetworkedShip attacker = null)
        {
            if (!IsAlive) return;
            Health -= damage;
            Health = Mathf.Max(0, Health);
            if (Health <= 0)
                Die();
        }

        public void AddXP(int amount)
        {
            CurrentXP += amount;
            CheckLevelUp();
        }

        public void AddLevelPenalty(int levels)
        {
            CurrentLevel = Mathf.Max(1, CurrentLevel - levels);
            CurrentXP = 0;
            int newTier = GetTierForLevel(CurrentLevel);
            if (newTier != CurrentTier) CurrentTier = newTier;
            MaxHealth = 100f * (1f + (CurrentLevel - 1) * 0.1f) * GetTierHealthMultiplier(CurrentTier);
            Health = MaxHealth;
            UpdateVisuals();
        }

        private void CheckLevelUp()
        {
            int xpNeeded = GetXPForLevel(CurrentLevel + 1);
            while (CurrentXP >= xpNeeded && CurrentLevel < 100)
            {
                CurrentXP -= xpNeeded;
                CurrentLevel++;
                xpNeeded = GetXPForLevel(CurrentLevel + 1);
                int newTier = GetTierForLevel(CurrentLevel);
                if (newTier != CurrentTier) CurrentTier = newTier;
            }
            MaxHealth = 100f * (1f + (CurrentLevel - 1) * 0.1f) * GetTierHealthMultiplier(CurrentTier);
            Health = MaxHealth;
            UpdateVisuals();
        }

        private void Die()
        {
            IsAlive = false;
            Health = 0;
        }

        public void UpdateVisuals()
        {
            if (_hullRenderer != null)
            {
                _hullRenderer.GetPropertyBlock(_hullProps);
                _hullProps.SetColor("_BaseColor", GetTierHullColor(CurrentTier));
                _hullRenderer.SetPropertyBlock(_hullProps);
            }
            if (_turretRenderer != null)
            {
                _turretRenderer.GetPropertyBlock(_turretProps);
                _turretProps.SetColor("_BaseColor", GetTierTurretColor(CurrentTier));
                _turretRenderer.SetPropertyBlock(_turretProps);
            }
        }

        private int GetXPForLevel(int level) => level * 100;
        private int GetTierForLevel(int level)
        {
            if (level <= 5) return 1;
            if (level <= 15) return 2;
            if (level <= 30) return 3;
            if (level <= 50) return 4;
            return 5;
        }
        private float GetTierHealthMultiplier(int tier)
        {
            return tier switch { 1 => 1f, 2 => 2.5f, 3 => 5f, 4 => 10f, 5 => 20f, _ => 1f };
        }
        private Color GetTierHullColor(int tier) => tier switch
        {
            1 => new Color(0.4f, 0.3f, 0.2f),
            2 => new Color(0.3f, 0.4f, 0.3f),
            3 => new Color(0.2f, 0.3f, 0.5f),
            4 => new Color(0.3f, 0.2f, 0.4f),
            5 => new Color(0.5f, 0.2f, 0.2f),
            _ => Color.white
        };
        private Color GetTierTurretColor(int tier) => tier switch
        {
            1 => new Color(0.5f, 0.4f, 0.3f),
            2 => new Color(0.4f, 0.5f, 0.4f),
            3 => new Color(0.3f, 0.4f, 0.6f),
            4 => new Color(0.4f, 0.3f, 0.5f),
            5 => new Color(0.6f, 0.3f, 0.3f),
            _ => Color.gray
        };
    }
}
