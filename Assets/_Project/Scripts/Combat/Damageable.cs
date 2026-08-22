using UnityEngine;

namespace OceanBattleRoyale.Combat
{
    public class Damageable : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        public float Health { get; private set; }
        public bool IsAlive { get; private set; }

        [Header("Damage")]
        [SerializeField] private float _armor = 0f;
        [SerializeField] private GameObject _deathEffectPrefab;

        [Header("Team")]
        [SerializeField] private int _teamId = 0;

        public float MaxHealth => _maxHealth;
        public int TeamId => _teamId;

        private void Start()
        {
            Health = _maxHealth;
            IsAlive = true;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            float finalDamage = Mathf.Max(0, damage - _armor);
            Health -= finalDamage;
            Health = Mathf.Max(0, Health);

            if (Health <= 0)
            {
                Die();
            }

            ShowDamageNumbers(finalDamage);
        }

        private void Die()
        {
            IsAlive = false;

            if (_deathEffectPrefab != null)
            {
                Instantiate(_deathEffectPrefab, transform.position, transform.rotation);
            }

            var gm = FindObjectOfType<OceanBattleRoyale.Core.GameManager>();
            if (gm != null)
            {
                gm.OnShipDied(gameObject);
            }

            Destroy(gameObject, 2f);
        }

        private void ShowDamageNumbers(float damage)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var textGO = new GameObject("DamageText");
                textGO.transform.SetParent(canvas.transform);
                textGO.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
                var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
                tmp.text = Mathf.CeilToInt(damage).ToString();
                tmp.fontSize = 24;
                tmp.color = Color.red;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                Destroy(textGO, 1f);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            Health = Mathf.Min(_maxHealth, Health + amount);
        }

        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            Health = maxHealth;
        }
    }
}
