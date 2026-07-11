using System;
using UnityEngine;

namespace BU.Workshop
{
    public class Gun : MonoBehaviour
    {
        [SerializeField]
        private Bullet _bulletPrefab;

        [SerializeField]
        private GameObject _muzzleRoot;

        [SerializeField]
        private float _fireRate = 3f;

        [SerializeField]
        private int _bulletPerFrame = 1;

        [SerializeField]
        private float _randomRotateRangeMin = -130f;

        [SerializeField]
        private float _randomRotateRangeMax = 130f;

        [SerializeField]
        private bool _useAutoFire = true;

        [SerializeField]
        private bool _usePooling;

        private float _nextFireTime;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _useAutoFire = !_useAutoFire;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                _usePooling = !_usePooling;
            }

            if (Input.GetMouseButtonDown(0) && !_useAutoFire)
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
                var direction = (worldPosition - transform.position).normalized;

                if (_usePooling)
                {
                    FireWithPool(direction);
                }
                else
                {
                    Fire(direction);
                }
            }

            if (Time.time >= _nextFireTime && _useAutoFire)
            {
                var randomAngle = UnityEngine.Random.Range(_randomRotateRangeMin, _randomRotateRangeMax);
                var direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

                for (int i = 0; i < _bulletPerFrame; i++)
                {
                    if (_usePooling)
                    {
                        FireWithPool(direction);
                    }
                    else
                    {
                        Fire(direction);
                    }
                }

                _nextFireTime = Time.time + 1f / _fireRate;
            }
        }

        private void FireWithPool(Vector2 direction)
        {
            var clampedRotation = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, direction), _randomRotateRangeMin, _randomRotateRangeMax);
            transform.rotation = Quaternion.Euler(0, 0, clampedRotation);

            Bullet bullet = PoolingManager.GetBulletPool();
            bullet.transform.position = _muzzleRoot.transform.position;
            bullet.transform.rotation = Quaternion.identity;
            bullet.WhenRequestedToDestroy += OnBulletPoolingRequestedToDestroy;
            bullet.transform.rotation = Quaternion.Euler(0, 0, clampedRotation);
        }

        private void Fire(Vector2 direction)
        {
            var clampedRotation = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, direction), _randomRotateRangeMin, _randomRotateRangeMax);
            transform.rotation = Quaternion.Euler(0, 0, clampedRotation);

            Bullet bullet = Instantiate(_bulletPrefab, _muzzleRoot.transform.position, Quaternion.identity);
            bullet.WhenRequestedToDestroy += OnBulletRequestedToDestroy;
            bullet.transform.rotation = Quaternion.Euler(0, 0, clampedRotation);
        }

        private void OnBulletRequestedToDestroy(Bullet bullet)
        {
            bullet.WhenRequestedToDestroy -= OnBulletRequestedToDestroy;
            Destroy(bullet.gameObject);
        }

        private void OnBulletPoolingRequestedToDestroy(Bullet bullet)
        {
            bullet.WhenRequestedToDestroy -= OnBulletPoolingRequestedToDestroy;
            PoolingManager.Release(bullet);
        }
    }
}