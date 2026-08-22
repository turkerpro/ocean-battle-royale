using UnityEngine;
using Fusion;

namespace OceanBattleRoyale.Combat
{
    public class Damageable : NetworkBehaviour
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [Networked] public float Health { get; private set; }
        [Networked] public NetworkBool IsAlive { get; private set; }

        [Header("Damage")]
        [SerializeField] private float _armor = 0f;
        [SerializeField] private GameObject _deathEffectPrefab;

        [Header("Team")]
        [SerializeField] private int _teamId = 0;

        public float MaxHealth => _maxHealth;
        public int TeamId => _teamId;

        public override void Spawned()
        {
            Health = _maxHealth;
            IsAlive = true;
        }

        public void TakeDamage(float damage, PlayerRef attacker)
        {
            if (!IsAlive || !Object.HasStateAuthority) return;

            float finalDamage = Mathf.Max(0, damage - _armor);
            Health -= finalDamage;
            Health = Mathf.Max(0, Health);

            if (Health <= 0)
            {
                Die(attacker);
            }

            RPC_ShowDamageNumbers(transform.position, finalDamage);
        }

        private void Die(PlayerRef killer)
        {
            IsAlive = false;

            if (_deathEffectPrefab != null)
            {
                Runner.Spawn(_deathEffectPrefab, transform.position, transform.rotation);
            }

            RPC_OnDeath(killer);
            Runner.Despawn(Object, 2f);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowDamageNumbers(Vector3 position, float damage)
        {
            // Spawn floating damage text
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var textGO = new GameObject("DamageText");
                textGO.transform.SetParent(canvas.transform);
                textGO.transform.position = Camera.main.WorldToScreenPoint(position + Vector3.up * 2f);
                var tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.text = Mathf.CeilToInt(damage).ToString();
                tmp.fontSize = 24;
                tmp.color = Color.red;
                tmp.alignment = TextAlignmentOptions.Center;
                Destroy(textGO, 1f);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnDeath(PlayerRef killer)
        {
            // Notify GameManager, update kill feed, etc.
            var gm = FindObjectOfType<OceanBattleRoyale.Core.GameManager>();
            if (gm != null && Object.HasInputAuthority)
            {
                gm.OnPlayerDied(Object.InputAuthority, killer);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || !Object.HasStateAuthority) return;
            Health = Mathf.Min(_maxHealth, Health + amount);
        }

        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            Health = maxHealth;
        }
    }
}
