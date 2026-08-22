using UnityEngine;
using Fusion;
using System;

namespace OceanBattleRoyale.Network
{
    public struct ShipInput : INetworkInput
    {
        public Vector2 Move;
        public Vector2 Aim;
        public NetworkBool Fire;
        public NetworkBool DeployMine;
        public byte WeaponSwitch;
    }

    [NetworkedBehaviour]
    public class NetworkedShip : NetworkBehaviour
    {
        [Header("Ship Configuration")]
        [Networked] public int CurrentTier { get; set; }
        [Networked] public int CurrentLevel { get; set; }
        [Networked] public int CurrentXP { get; set; }
        [Networked] public float Health { get; set; }
        [Networked] public float MaxHealth { get; set; }
        [Networked] public byte ActiveWeaponSlot { get; set; }
        [Networked] public NetworkBool IsAlive { get; set; }

        [Header("Visual")]
        [SerializeField] private Renderer _hullRenderer;
        [SerializeField] private Renderer _turretRenderer;
        [SerializeField] private TrailRenderer _wakeTrail;

        private ShipPhysics _physics;
        private MaterialPropertyBlock _hullProps;
        private MaterialPropertyBlock _turretProps;

        public override void Spawned()
        {
            _physics = GetComponent<ShipPhysics>();
            _hullProps = new MaterialPropertyBlock();
            _turretProps = new MaterialPropertyBlock();

            if (Object.HasInputAuthority)
            {
                InitializeLocalShip();
            }

            IsAlive = true;
        }

        private void InitializeLocalShip()
        {
            CurrentTier = 1;
            CurrentLevel = 1;
            CurrentXP = 0;
            MaxHealth = 100f;
            Health = MaxHealth;
            ActiveWeaponSlot = 0;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out ShipInput input))
            {
                _physics.Simulate(input, Runner.DeltaTime);

                if (Object.HasInputAuthority)
                {
                    transform.position = _physics.Position;
                    transform.rotation = _physics.Rotation;
                }
                else
                {
                    _physics.SetTarget(transform.position, transform.rotation);
                }
            }

            if (Object.HasStateAuthority)
            {
                transform.position = _physics.Position;
                transform.rotation = _physics.Rotation;
            }
        }

        public override void Render()
        {
            if (!Object.HasStateAuthority && _physics != null)
            {
                _physics.Interpolate(Runner.InterpolationFactor);
                transform.position = _physics.RenderPosition;
                transform.rotation = _physics.RenderRotation;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestFire(Vector3 aimDirection)
        {
            if (!IsAlive) return;
            // Weapon system will handle actual firing
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestDeployMine()
        {
            if (!IsAlive) return;
            // Mine system will handle deployment
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_TakeDamage(float damage, PlayerRef attacker)
        {
            if (!IsAlive) return;

            Health -= damage;
            Health = Mathf.Max(0, Health);

            if (Health <= 0)
            {
                Die(attacker);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AddXP(int amount)
        {
            CurrentXP += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int xpForNextLevel = GetXPForLevel(CurrentLevel + 1);
            if (CurrentXP >= xpForNextLevel)
            {
                CurrentLevel++;
                CurrentXP -= xpForNextLevel;
                ApplyLevelStats();

                int newTier = GetTierForLevel(CurrentLevel);
                if (newTier != CurrentTier)
                {
                    UpgradeToTier(newTier);
                }
            }
        }

        private void ApplyLevelStats()
        {
            // Base stats scale with level
            float healthMultiplier = 1f + (CurrentLevel - 1) * 0.1f;
            MaxHealth = 100f * healthMultiplier * GetTierHealthMultiplier(CurrentTier);
            Health = MaxHealth;
        }

        private void UpgradeToTier(int newTier)
        {
            CurrentTier = newTier;
            ApplyLevelStats();
            UpdateVisuals();
        }

        private void UpdateVisuals()
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

        private void Die(PlayerRef killer)
        {
            IsAlive = false;
            // Disable collider, renderer, etc.
            // Respawn logic handled by GameManager
        }

        private int GetXPForLevel(int level)
        {
            return level * 100;
        }

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

        private Color GetTierHullColor(int tier)
        {
            return tier switch
            {
                1 => new Color(0.4f, 0.3f, 0.2f),
                2 => new Color(0.3f, 0.4f, 0.3f),
                3 => new Color(0.2f, 0.3f, 0.5f),
                4 => new Color(0.3f, 0.2f, 0.4f),
                5 => new Color(0.5f, 0.2f, 0.2f),
                _ => Color.white
            };
        }

        private Color GetTierTurretColor(int tier)
        {
            return tier switch
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
}
