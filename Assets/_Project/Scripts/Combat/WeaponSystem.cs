using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace OceanBattleRoyale.Combat
{
    public enum WeaponType { Light, Medium, Heavy }
    public enum FireMode { Automatic, SemiAuto, Burst, Charge }

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
    }

    public class WeaponSystem : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponData[] _availableWeapons;
        [SerializeField] private Transform[] _firePoints;
        [SerializeField] private AudioSource _audioSource;

        [Networked] private byte _currentWeaponIndex { get; set; }
        [Networked] private int _currentAmmo { get; set; }
        [Networked] private NetworkBool _isReloading { get; set; }
        [Networked] private float _nextFireTime { get; set; }

        private WeaponData _currentWeapon => _currentWeaponIndex < _availableWeapons.Length ? _availableWeapons[_currentWeaponIndex] : null;
        private Dictionary<WeaponData, int> _ammoReserves = new Dictionary<WeaponData, int>();

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                InitializeAmmo();
                _currentWeaponIndex = 0;
            }
        }

        private void InitializeAmmo()
        {
            foreach (var weapon in _availableWeapons)
            {
                if (weapon != null && weapon.MaxAmmo > 0)
                {
                    _ammoReserves[weapon] = weapon.MaxAmmo;
                }
            }
            UpdateCurrentAmmo();
        }

        private void UpdateCurrentAmmo()
        {
            var weapon = _currentWeapon;
            if (weapon != null && weapon.MaxAmmo > 0 && _ammoReserves.TryGetValue(weapon, out int ammo))
            {
                _currentAmmo = ammo;
            }
            else
            {
                _currentAmmo = -1;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            if (_isReloading)
            {
                // Reload handled by coroutine on host
                return;
            }

            if (_nextFireTime > 0 && Runner.Time < _nextFireTime)
            {
                return;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestFire(Vector3 aimDirection)
        {
            if (_isReloading || _currentWeapon == null) return;

            if (_currentWeapon.MaxAmmo > 0 && _currentAmmo <= 0)
            {
                StartReload();
                return;
            }

            if (Runner.Time < _nextFireTime) return;

            Fire(aimDirection);
            _nextFireTime = Runner.Time + _currentWeapon.Cooldown;

            if (_currentWeapon.MaxAmmo > 0)
            {
                _currentAmmo--;
                _ammoReserves[_currentWeapon] = _currentAmmo;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestReload()
        {
            if (_isReloading || _currentWeapon == null || _currentWeapon.MaxAmmo <= 0) return;
            if (_currentAmmo >= _currentWeapon.MaxAmmo) return;

            StartReload();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestSwitchWeapon(byte weaponIndex)
        {
            if (weaponIndex >= _availableWeapons.Length) return;
            if (_isReloading) return;
            if (weaponIndex == _currentWeaponIndex) return;

            _currentWeaponIndex = weaponIndex;
            UpdateCurrentAmmo();
            _nextFireTime = Runner.Time + 0.5f; // Switch delay
        }

        private void Fire(Vector3 aimDirection)
        {
            var weapon = _currentWeapon;
            if (weapon == null || _firePoints.Length == 0) return;

            for (int i = 0; i < weapon.ProjectilesPerShot; i++)
            {
                Vector3 spreadDir = aimDirection;
                if (weapon.SpreadAngle > 0)
                {
                    float angle = Random.Range(-weapon.SpreadAngle, weapon.SpreadAngle);
                    spreadDir = Quaternion.Euler(0, angle, 0) * aimDirection;
                }

                SpawnProjectile(_firePoints[0].position, spreadDir, weapon);
            }

            PlayFireEffects(weapon);
        }

        private void SpawnProjectile(Vector3 position, Vector3 direction, WeaponData weapon)
        {
            if (weapon.ProjectilePrefab == null) return;

            var projectile = Runner.Spawn(weapon.ProjectilePrefab, position, Quaternion.LookRotation(direction), Object.InputAuthority);
            var projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(direction * weapon.ProjectileSpeed, weapon.Damage, weapon.Range, weapon.Homing);
            }
        }

        private void PlayFireEffects(WeaponData weapon)
        {
            if (weapon.FireSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(weapon.FireSound);
            }

            if (weapon.MuzzleFlashPrefab != null && _firePoints.Length > 0)
            {
                var flash = Instantiate(weapon.MuzzleFlashPrefab, _firePoints[0].position, _firePoints[0].rotation);
                flash.transform.SetParent(_firePoints[0]);
                Destroy(flash, 0.1f);
            }
        }

        private void StartReload()
        {
            _isReloading = true;
            StartCoroutine(ReloadCoroutine());
        }

        private System.Collections.IEnumerator ReloadCoroutine()
        {
            var weapon = _currentWeapon;
            if (weapon == null || weapon.ReloadSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(weapon.ReloadSound);
            }

            yield return new WaitForSeconds(weapon != null ? weapon.ReloadTime : 2f);

            if (weapon != null && weapon.MaxAmmo > 0)
            {
                _currentAmmo = weapon.MaxAmmo;
                _ammoReserves[weapon] = _currentAmmo;
            }

            _isReloading = false;
        }

        public WeaponData GetCurrentWeapon() => _currentWeapon;
        public int GetCurrentAmmo() => _currentAmmo;
        public int GetMaxAmmo() => _currentWeapon?.MaxAmmo ?? -1;
        public bool IsReloading() => _isReloading;
        public byte GetCurrentWeaponIndex() => _currentWeaponIndex;
        public int GetWeaponCount() => _availableWeapons.Length;
        public WeaponData GetWeaponData(int index) => index < _availableWeapons.Length ? _availableWeapons[index] : null;
    }

    public class Projectile : NetworkBehaviour
    {
        [Networked] private Vector3 _velocity { get; set; }
        [Networked] private int _damage { get; set; }
        [Networked] private float _remainingDistance { get; set; }
        [Networked] private NetworkBool _homing { get; set; }
        [Networked] private NetworkBool _hasHit { get; set; }

        private float _lifeTime = 10f;

        public void Initialize(Vector3 velocity, int damage, float range, bool homing)
        {
            _velocity = velocity;
            _damage = damage;
            _remainingDistance = range;
            _homing = homing;
        }

        public override void FixedUpdateNetwork()
        {
            if (_hasHit) return;

            Vector3 move = _velocity * Runner.DeltaTime;
            float moveDist = move.magnitude;

            if (moveDist > _remainingDistance)
            {
                move = move.normalized * _remainingDistance;
                _remainingDistance = 0;
            }
            else
            {
                _remainingDistance -= moveDist;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, move.normalized, out hit, moveDist))
            {
                OnHit(hit);
                return;
            }

            transform.position += move;

            if (_remainingDistance <= 0 || _lifeTime <= 0)
            {
                Runner.Despawn(Object);
            }

            _lifeTime -= Runner.DeltaTime;
        }

        private void OnHit(RaycastHit hit)
        {
            _hasHit = true;

            var damageable = hit.collider.GetComponentInParent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage, Object.InputAuthority);
            }

            Runner.Despawn(Object);
        }
    }
}
