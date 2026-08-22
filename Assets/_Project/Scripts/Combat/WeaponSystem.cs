using UnityEngine;
using System.Collections.Generic;

namespace OceanBattleRoyale.Combat
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponData[] _availableWeapons;
        [SerializeField] private Transform[] _firePoints;
        [SerializeField] private AudioSource _audioSource;

        private byte _currentWeaponIndex;
        private int _currentAmmo;
        private bool _isReloading;
        private float _nextFireTime;

        private WeaponData _currentWeapon => _currentWeaponIndex < _availableWeapons.Length ? _availableWeapons[_currentWeaponIndex] : null;
        private Dictionary<WeaponData, int> _ammoReserves = new Dictionary<WeaponData, int>();

        private void Start()
        {
            InitializeAmmo();
            _currentWeaponIndex = 0;
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

        private void Update()
        {
            if (_isReloading) return;
            if (_nextFireTime > 0 && Time.time < _nextFireTime) return;
        }

        public void RequestFire(Vector3 aimDirection)
        {
            if (_isReloading || _currentWeapon == null) return;

            if (_currentWeapon.MaxAmmo > 0 && _currentAmmo <= 0)
            {
                StartReload();
                return;
            }

            if (Time.time < _nextFireTime) return;

            Fire(aimDirection);
            _nextFireTime = Time.time + _currentWeapon.Cooldown;

            if (_currentWeapon.MaxAmmo > 0)
            {
                _currentAmmo--;
                _ammoReserves[_currentWeapon] = _currentAmmo;
            }
        }

        public void RequestReload()
        {
            if (_isReloading || _currentWeapon == null || _currentWeapon.MaxAmmo <= 0) return;
            if (_currentAmmo >= _currentWeapon.MaxAmmo) return;

            StartReload();
        }

        public void RequestSwitchWeapon(byte weaponIndex)
        {
            if (weaponIndex >= _availableWeapons.Length) return;
            if (_isReloading) return;
            if (weaponIndex == _currentWeaponIndex) return;

            _currentWeaponIndex = weaponIndex;
            UpdateCurrentAmmo();
            _nextFireTime = Time.time + 0.5f;
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

            GameObject projectile = Instantiate(weapon.ProjectilePrefab, position, Quaternion.LookRotation(direction));
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
            if (weapon != null && weapon.ReloadSound != null && _audioSource != null)
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

    public class Projectile : MonoBehaviour
    {
        private Vector3 _velocity;
        private int _damage;
        private float _remainingDistance;
        private bool _homing;
        private bool _hasHit;

        private float _lifeTime = 10f;

        public void Initialize(Vector3 velocity, int damage, float range, bool homing)
        {
            _velocity = velocity;
            _damage = damage;
            _remainingDistance = range;
            _homing = homing;
        }

        private void Update()
        {
            if (_hasHit) return;

            float deltaTime = Time.deltaTime;
            Vector3 move = _velocity * deltaTime;
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
                Destroy(gameObject);
            }

            _lifeTime -= deltaTime;
        }

        private void OnHit(RaycastHit hit)
        {
            _hasHit = true;

            var damageable = hit.collider.GetComponentInParent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}
