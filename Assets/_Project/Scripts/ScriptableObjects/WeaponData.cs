using UnityEngine;

namespace OceanBattleRoyale.Combat
{
    [CreateAssetMenu(menuName = "Ocean Battle Royale/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string DisplayName;
        public WeaponType Type;
        public FireMode FireMode;
        public Sprite Icon;

        [Header("Stats")]
        public float Range = 50f;
        public int Damage = 20;
        public float Cooldown = 0.5f;
        public float ProjectileSpeed = 100f;
        public int ProjectilesPerShot = 1;
        public float SpreadAngle = 0f;
        public bool Homing = false;
        public int MaxAmmo = -1;
        public float ReloadTime = 2f;

        [Header("Visuals")]
        public GameObject ProjectilePrefab;
        public GameObject MuzzleFlashPrefab;
        public AudioClip FireSound;
        public AudioClip ReloadSound;

        public static WeaponData[] GetDefaultWeapons()
        {
            var weapons = new WeaponData[5];

            // Cannon (Heavy)
            weapons[0] = CreateInstance<WeaponData>();
            weapons[0].DisplayName = "Top";
            weapons[0].Type = WeaponType.Heavy;
            weapons[0].FireMode = FireMode.SemiAuto;
            weapons[0].Range = 50f;
            weapons[0].Damage = 80;
            weapons[0].Cooldown = 2.5f;
            weapons[0].ProjectileSpeed = 80f;
            weapons[0].ProjectilesPerShot = 1;
            weapons[0].SpreadAngle = 0f;
            weapons[0].MaxAmmo = 20;
            weapons[0].ReloadTime = 3f;

            // Machine Gun (Medium)
            weapons[1] = CreateInstance<WeaponData>();
            weapons[1].DisplayName = "Makinalı";
            weapons[1].Type = WeaponType.Medium;
            weapons[1].FireMode = FireMode.Automatic;
            weapons[1].Range = 30f;
            weapons[1].Damage = 15;
            weapons[1].Cooldown = 0.1f;
            weapons[1].ProjectileSpeed = 120f;
            weapons[1].ProjectilesPerShot = 1;
            weapons[1].SpreadAngle = 3f;
            weapons[1].MaxAmmo = 200;
            weapons[1].ReloadTime = 2f;

            // Missile (Heavy)
            weapons[2] = CreateInstance<WeaponData>();
            weapons[2].DisplayName = "Füzeler";
            weapons[2].Type = WeaponType.Heavy;
            weapons[2].FireMode = FireMode.SemiAuto;
            weapons[2].Range = 80f;
            weapons[2].Damage = 120;
            weapons[2].Cooldown = 5f;
            weapons[2].ProjectileSpeed = 60f;
            weapons[2].ProjectilesPerShot = 1;
            weapons[2].Homing = true;
            weapons[2].MaxAmmo = 8;
            weapons[2].ReloadTime = 4f;

            // Laser (Light)
            weapons[3] = CreateInstance<WeaponData>();
            weapons[3].DisplayName = "Lazer";
            weapons[3].Type = WeaponType.Light;
            weapons[3].FireMode = FireMode.Automatic;
            weapons[3].Range = 100f;
            weapons[3].Damage = 40;
            weapons[3].Cooldown = 0.05f;
            weapons[3].ProjectileSpeed = 200f;
            weapons[3].ProjectilesPerShot = 1;
            weapons[3].MaxAmmo = -1;
            weapons[3].ReloadTime = 0f;

            // Torpedo (Heavy)
            weapons[4] = CreateInstance<WeaponData>();
            weapons[4].DisplayName = "Torpedo";
            weapons[4].Type = WeaponType.Heavy;
            weapons[4].FireMode = FireMode.SemiAuto;
            weapons[4].Range = 40f;
            weapons[4].Damage = 200;
            weapons[4].Cooldown = 8f;
            weapons[4].ProjectileSpeed = 40f;
            weapons[4].ProjectilesPerShot = 1;
            weapons[4].MaxAmmo = 4;
            weapons[4].ReloadTime = 5f;

            return weapons;
        }
    }
}
