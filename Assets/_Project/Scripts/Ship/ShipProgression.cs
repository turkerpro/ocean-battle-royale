using UnityEngine;
using Fusion;

namespace OceanBattleRoyale.Ship
{
    public class ShipProgression : NetworkBehaviour
    {
        [Header("Level Curve")]
        [SerializeField] private AnimationCurve _xpCurve;
        [SerializeField] private ShipTierData[] _tierData;

        [Networked] public int CurrentLevel { get; private set; }
        [Networked] public int CurrentXP { get; private set; }
        [Networked] public int CurrentTier { get; private set; }

        private NetworkedShip _networkedShip;
        private ShipPhysics _shipPhysics;

        public override void Spawned()
        {
            _networkedShip = GetComponent<NetworkedShip>();
            _shipPhysics = GetComponent<ShipPhysics>();

            if (Object.HasStateAuthority)
            {
                CurrentLevel = 1;
                CurrentXP = 0;
                CurrentTier = 1;
                ApplyTierStats();
            }
        }

        public void AddXP(int amount)
        {
            if (!Object.HasStateAuthority) return;

            CurrentXP += amount;
            CheckLevelUp();
        }

        public void AddLevelPenalty(int levels)
        {
            if (!Object.HasStateAuthority) return;

            int newLevel = Mathf.Max(1, CurrentLevel - levels);
            int xpLoss = CurrentXP;

            CurrentLevel = newLevel;
            CurrentXP = 0;

            int newTier = GetTierForLevel(CurrentLevel);
            if (newTier != CurrentTier)
            {
                CurrentTier = newTier;
                ApplyTierStats();
                UpdateVisuals();
            }

            _networkedShip.RPC_AddXP(0); // Sync
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
                if (newTier != CurrentTier)
                {
                    CurrentTier = newTier;
                    ApplyTierStats();
                    UpdateVisuals();
                }
            }

            _networkedShip.RPC_AddXP(0); // Sync
        }

        private void ApplyTierStats()
        {
            if (_shipPhysics != null)
            {
                _shipPhysics.ApplyTierStats(CurrentTier);
            }

            var tier = GetTierData(CurrentTier);
            if (tier != null && _networkedShip != null)
            {
                _networkedShip.MaxHealth = tier.MaxHealth;
                _networkedShip.Health = tier.MaxHealth;
            }
        }

        private void UpdateVisuals()
        {
            _networkedShip?.UpdateVisuals();
        }

        private int GetXPForLevel(int level)
        {
            if (_xpCurve.length > 0)
            {
                return Mathf.RoundToInt(_xpCurve.Evaluate(level));
            }
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

        private ShipTierData GetTierData(int tier)
        {
            foreach (var t in _tierData)
            {
                if (t.Tier == tier) return t;
            }
            return null;
        }

        public int GetXPForNextLevel() => GetXPForLevel(CurrentLevel + 1);
        public float GetLevelProgress() => (float)CurrentXP / GetXPForNextLevel();
    }
}
