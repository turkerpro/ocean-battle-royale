using UnityEngine;
using OceanBattleRoyale.Network;

namespace OceanBattleRoyale.Combat
{
    public class MineSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MineData[] _availableMines;
        [SerializeField] private Transform _deployPoint;
        [SerializeField] private AudioSource _audioSource;

        private byte _currentMineIndex;
        private int _activeMinesCount;
        private float _nextDeployTime;

        private MineData _currentMine => _currentMineIndex < _availableMines.Length ? _availableMines[_currentMineIndex] : null;

        private void Start()
        {
            _currentMineIndex = 0;
            _activeMinesCount = 0;
        }

        public void RequestDeployMine()
        {
            if (_currentMine == null) return;
            if (Time.time < _nextDeployTime) return;
            if (_activeMinesCount >= _currentMine.MaxMines) return;

            DeployMine();
            _nextDeployTime = Time.time + _currentMine.Cooldown;
        }

        public void RequestSwitchMine(byte mineIndex)
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

            GameObject mineObj = Instantiate(mine.MinePrefab, deployPos, Quaternion.identity);
            var mineScript = mineObj.GetComponent<Mine>();
            if (mineScript != null)
            {
                mineScript.Initialize(mine, gameObject);
            }

            _activeMinesCount++;

            if (mine.DeploySound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(mine.DeploySound);
            }
        }

        public void OnMineDestroyed()
        {
            _activeMinesCount = Mathf.Max(0, _activeMinesCount - 1);
        }

        public MineData GetCurrentMine() => _currentMine;
        public byte GetCurrentMineIndex() => _currentMineIndex;
        public int GetMaxMines() => _currentMine?.MaxMines ?? 3;
        public int GetActiveMinesCount() => _activeMinesCount;
        public float GetCooldownRemaining() => Mathf.Max(0, _nextDeployTime - Time.time);
    }

    public class Mine : MonoBehaviour
    {
        private float _lifeTimer;
        private bool _triggered;
        private GameObject _owner;
        private MineData _data;
        private MineSystem _ownerMineSystem;

        public void Initialize(MineData data, GameObject owner)
        {
            _data = data;
            _owner = owner;
            _lifeTimer = data.Lifetime;
            _triggered = false;

            if (owner != null)
            {
                _ownerMineSystem = owner.GetComponent<MineSystem>();
            }
        }

        private void Update()
        {
            if (_triggered) return;

            _lifeTimer -= Time.deltaTime;

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
                if (ship != null && ship.gameObject != _owner && ship.IsAlive)
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
                transform.position += Vector3.forward * 0.5f * Time.deltaTime;
            }
        }

        private void Trigger(NetworkedShip target)
        {
            _triggered = true;

            target.TakeDamage(_data.Damage);

            var targetProgression = target.GetComponent<Ship.ShipProgression>();
            if (targetProgression != null)
            {
                targetProgression.AddLevelPenalty(_data.LevelPenalty);
            }

            if (_data.ExplosionPrefab != null)
            {
                Instantiate(_data.ExplosionPrefab, transform.position, Quaternion.identity);
            }

            if (_data.ExplosionSound != null)
            {
                AudioSource.PlayClipAtPoint(_data.ExplosionSound, transform.position);
            }

            if (_ownerMineSystem != null)
            {
                _ownerMineSystem.OnMineDestroyed();
            }

            Destroy(gameObject);
        }

        private void Expire()
        {
            if (_ownerMineSystem != null)
            {
                _ownerMineSystem.OnMineDestroyed();
            }
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_triggered && _data != null && _data.Type == MineType.Contact)
            {
                var ship = other.GetComponentInParent<NetworkedShip>();
                if (ship != null && ship.gameObject != _owner && ship.IsAlive)
                {
                    Trigger(ship);
                }
            }
        }
    }
}
