using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace OceanBattleRoyale.Combat
{
    public enum MineType { Contact, Proximity, Magnetic, Drift }

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
    }

    public class MineSystem : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MineData[] _availableMines;
        [SerializeField] private Transform _deployPoint;
        [SerializeField] private AudioSource _audioSource;

        [Networked] private byte _currentMineIndex { get; set; }
        [Networked] private int _activeMinesCount { get; set; }
        [Networked] private float _nextDeployTime { get; set; }

        private MineData _currentMine => _currentMineIndex < _availableMines.Length ? _availableMines[_currentMineIndex] : null;

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                _currentMineIndex = 0;
                _activeMinesCount = 0;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestDeployMine()
        {
            if (_currentMine == null) return;
            if (Runner.Time < _nextDeployTime) return;
            if (_activeMinesCount >= _currentMine.MaxMines) return;

            DeployMine();
            _nextDeployTime = Runner.Time + _currentMine.Cooldown;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestSwitchMine(byte mineIndex)
        {
            if (mineIndex >= _availableMines.Length) return;
            if (mineIndex == _currentMineIndex) return;

            _currentMineIndex = mineIndex;
        }

        private void DeployMine()
        {
            var mine = _currentMine;
            if (mine == null || mine.MinePrefab == null || _deployPoint == null) return;

            Vector3 deployPos = _deployPoint.position;
            deployPos.y = 0.5f;

            var mineObj = Runner.Spawn(mine.MinePrefab, deployPos, Quaternion.identity, Object.InputAuthority);
            var mineScript = mineObj.GetComponent<Mine>();
            if (mineScript != null)
            {
                mineScript.Initialize(mine, Object.InputAuthority);
            }

            _activeMinesCount++;

            if (mine.DeploySound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(mine.DeploySound);
            }
        }

        public void OnMineDestroyed()
        {
            if (Object.HasStateAuthority)
            {
                _activeMinesCount = Mathf.Max(0, _activeMinesCount - 1);
            }
        }

        public MineData GetCurrentMine() => _currentMine;
        public byte GetCurrentMineIndex() => _currentMineIndex;
        public int GetMaxMines() => _currentMine?.MaxMines ?? 3;
        public int GetActiveMinesCount() => _activeMinesCount;
        public float GetCooldownRemaining() => Mathf.Max(0, _nextDeployTime - Runner.Time);
    }

    public class Mine : NetworkBehaviour
    {
        [Networked] private float _lifeTimer { get; set; }
        [Networked] private NetworkBool _triggered { get; set; }
        [Networked] private PlayerRef _owner { get; set; }
        private MineData _data;
        private MineSystem _ownerMineSystem;

        public void Initialize(MineData data, PlayerRef owner)
        {
            _data = data;
            _owner = owner;
            _lifeTimer = data.Lifetime;
            _triggered = false;

            var ownerObj = Runner.GetPlayerObject(owner);
            if (ownerObj != null)
            {
                _ownerMineSystem = ownerObj.GetComponent<MineSystem>();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (_triggered) return;

            _lifeTimer -= Runner.DeltaTime;

            if (_lifeTimer <= 0)
            {
                Expire();
                return;
            }

            CheckTrigger();
        }

        private void CheckTrigger()
        {
            if (_data == null) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, _data.TriggerRadius);
            foreach (var hit in hits)
            {
                var ship = hit.GetComponentInParent<NetworkedShip>();
                if (ship != null && ship.Object.InputAuthority != _owner && ship.IsAlive)
                {
                    Trigger(ship);
                    return;
                }

                if (_data.Type == MineType.Magnetic)
                {
                    var rb = hit.GetComponent<Rigidbody>();
                    if (rb != null && rb.gameObject != gameObject)
                    {
                        Vector3 pullDir = (transform.position - rb.position).normalized;
                        rb.AddForce(pullDir * 50f, ForceMode.Acceleration);
                    }
                }
            }

            if (_data.Type == MineType.Drift)
            {
                transform.position += Vector3.forward * 0.5f * Runner.DeltaTime;
            }
        }

        private void Trigger(NetworkedShip target)
        {
            _triggered = true;

            target.RPC_TakeDamage(_data.Damage, _owner);

            var targetProgression = target.GetComponent<ShipProgression>();
            if (targetProgression != null)
            {
                targetProgression.AddLevelPenalty(_data.LevelPenalty);
            }

            if (_data.ExplosionPrefab != null)
            {
                Runner.Spawn(_data.ExplosionPrefab, transform.position, Quaternion.identity);
            }

            if (_data.ExplosionSound != null)
            {
                AudioSource.PlayClipAtPoint(_data.ExplosionSound, transform.position);
            }

            Runner.Despawn(Object);
        }

        private void Expire()
        {
            if (_ownerMineSystem != null)
            {
                _ownerMineSystem.OnMineDestroyed();
            }
            Runner.Despawn(Object);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_triggered && _data.Type == MineType.Contact)
            {
                var ship = other.GetComponentInParent<NetworkedShip>();
                if (ship != null && ship.Object.InputAuthority != _owner && ship.IsAlive)
                {
                    Trigger(ship);
                }
            }
        }
    }
}
