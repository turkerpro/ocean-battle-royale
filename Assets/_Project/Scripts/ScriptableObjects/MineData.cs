using UnityEngine;

namespace OceanBattleRoyale.Combat
{
    [CreateAssetMenu(menuName = "Ocean Battle Royale/Mine Data")]
    public class MineData : ScriptableObject
    {
        public string DisplayName;
        public MineType Type;
        public Sprite Icon;

        [Header("Stats")]
        public float Damage = 100f;
        public float TriggerRadius = 5f;
        public float Lifetime = 60f;
        public int LevelPenalty = 1;
        public float Cooldown = 10f;
        public int MaxMines = 3;

        [Header("Visuals")]
        public GameObject MinePrefab;
        public GameObject ExplosionPrefab;
        public AudioClip DeploySound;
        public AudioClip ExplosionSound;

        public static MineData[] GetDefaultMines()
        {
            var mines = new MineData[4];

            mines[0] = CreateInstance<MineData>();
            mines[0].DisplayName = "Temas Mayını";
            mines[0].Type = MineType.Contact;
            mines[0].Damage = 150f;
            mines[0].TriggerRadius = 2f;
            mines[0].Lifetime = 120f;
            mines[0].LevelPenalty = 1;
            mines[0].Cooldown = 10f;
            mines[0].MaxMines = 3;

            mines[1] = CreateInstance<MineData>();
            mines[1].DisplayName = "Yakınlık Mayını";
            mines[1].Type = MineType.Proximity;
            mines[1].Damage = 100f;
            mines[1].TriggerRadius = 10f;
            mines[1].Lifetime = 60f;
            mines[1].LevelPenalty = 1;
            mines[1].Cooldown = 15f;
            mines[1].MaxMines = 2;

            mines[2] = CreateInstance<MineData>();
            mines[2].DisplayName = "Manyetik Mayın";
            mines[2].Type = MineType.Magnetic;
            mines[2].Damage = 80f;
            mines[2].TriggerRadius = 8f;
            mines[2].Lifetime = 30f;
            mines[2].LevelPenalty = 2;
            mines[2].Cooldown = 20f;
            mines[2].MaxMines = 1;

            mines[3] = CreateInstance<MineData>();
            mines[3].DisplayName = "Sürü Mayını";
            mines[3].Type = MineType.Drift;
            mines[3].Damage = 50f;
            mines[3].TriggerRadius = 3f;
            mines[3].Lifetime = 120f;
            mines[3].LevelPenalty = 1;
            mines[3].Cooldown = 8f;
            mines[3].MaxMines = 5;

            return mines;
        }
    }
}
