using UnityEngine;

namespace OceanBattleRoyale.Ship
{
    [CreateAssetMenu(menuName = "Ocean Battle Royale/Ship Tier Data")]
    public class ShipTierData : ScriptableObject
    {
        public int Tier;
        public string DisplayName;
        public int MinLevel;
        public int MaxLevel;
        public float MaxHealth;
        public float MaxSpeed;
        public float Acceleration;
        public float TurnRate;
        public float Mass;
        public int WeaponSlotsLight;
        public int WeaponSlotsMedium;
        public int WeaponSlotsHeavy;
        public GameObject Prefab;
        public Material BaseMaterial;
        public Color HullColor;
        public Color TurretColor;

        public static ShipTierData GetDefaultTier(int tier)
        {
            var data = CreateInstance<ShipTierData>();
            data.Tier = tier;
            switch (tier)
            {
                case 1:
                    data.DisplayName = "Çektirme Teknesi";
                    data.MinLevel = 1; data.MaxLevel = 5;
                    data.MaxHealth = 100; data.MaxSpeed = 12; data.Acceleration = 3;
                    data.TurnRate = 30; data.Mass = 5000;
                    data.WeaponSlotsLight = 1; data.WeaponSlotsMedium = 0; data.WeaponSlotsHeavy = 0;
                    data.HullColor = new Color(0.4f, 0.3f, 0.2f);
                    data.TurretColor = new Color(0.5f, 0.4f, 0.3f);
                    break;
                case 2:
                    data.DisplayName = "Korvet";
                    data.MinLevel = 6; data.MaxLevel = 15;
                    data.MaxHealth = 250; data.MaxSpeed = 10; data.Acceleration = 2;
                    data.TurnRate = 20; data.Mass = 15000;
                    data.WeaponSlotsLight = 1; data.WeaponSlotsMedium = 1; data.WeaponSlotsHeavy = 0;
                    data.HullColor = new Color(0.3f, 0.4f, 0.3f);
                    data.TurretColor = new Color(0.4f, 0.5f, 0.4f);
                    break;
                case 3:
                    data.DisplayName = "Fırkateyn";
                    data.MinLevel = 16; data.MaxLevel = 30;
                    data.MaxHealth = 500; data.MaxSpeed = 9; data.Acceleration = 1.5f;
                    data.TurnRate = 15; data.Mass = 30000;
                    data.WeaponSlotsLight = 1; data.WeaponSlotsMedium = 1; data.WeaponSlotsHeavy = 1;
                    data.HullColor = new Color(0.2f, 0.3f, 0.5f);
                    data.TurretColor = new Color(0.3f, 0.4f, 0.6f);
                    break;
                case 4:
                    data.DisplayName = "Kruvazör";
                    data.MinLevel = 31; data.MaxLevel = 50;
                    data.MaxHealth = 1000; data.MaxSpeed = 8; data.Acceleration = 1;
                    data.TurnRate = 10; data.Mass = 50000;
                    data.WeaponSlotsLight = 2; data.WeaponSlotsMedium = 2; data.WeaponSlotsHeavy = 1;
                    data.HullColor = new Color(0.3f, 0.2f, 0.4f);
                    data.TurretColor = new Color(0.4f, 0.3f, 0.5f);
                    break;
                case 5:
                    data.DisplayName = "Savaş Gemisi";
                    data.MinLevel = 51; data.MaxLevel = 100;
                    data.MaxHealth = 2000; data.MaxSpeed = 6; data.Acceleration = 0.5f;
                    data.TurnRate = 6; data.Mass = 80000;
                    data.WeaponSlotsLight = 2; data.WeaponSlotsMedium = 2; data.WeaponSlotsHeavy = 2;
                    data.HullColor = new Color(0.5f, 0.2f, 0.2f);
                    data.TurretColor = new Color(0.6f, 0.3f, 0.3f);
                    break;
            }
            return data;
        }
    }
}
