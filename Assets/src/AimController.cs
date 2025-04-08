using Assets.src;
using UnityEngine;

namespace LD57
{
    public class AimController : MonoBehaviour
    {
        [Tooltip("How quickly the aim controller zeroes in on the target.")]
        public float trackingSpeed = 1f;
        [Tooltip("The rate of fire.")]
        public float shootDelay = 0.3f;
        [Tooltip("Controls the spread of the bullets when fired.")]
        public float bulletSprayCone = 15f;
        [Tooltip("Maximum bullets that can be active at any given time.")]
        public int maxBullets = 10;
        [Tooltip("AI controls the aiming")]
        public bool autoAim;
        [Tooltip("The point where the bullets are spawned.")]
        public Transform bulletSpawnPoint;
        [Tooltip("The sprite to use for bullets.")]
        public Sprite sprite;
        [Tooltip("The owner of this aim controller.")]
        public Killable owner;

        private BulletController m_bulletPrefab;
        private BulletController[] m_bullets;
        private int m_bulletIndex = 0;
        private float m_shootDelay = 0;
        private bool m_canShoot;
        private bool m_isShooting;
        private AutoTargetSelector m_autoTargetSelector;
        private Transform m_playerTransform;

        public void StartShooting()
        {
            m_isShooting = true;
        }

        public void StopShooting()
        {
            m_isShooting = false;
        }

        void Awake()
        {
            m_bulletPrefab = Resources.Load<BulletController>("Prefabs/Bullet");
            m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Start()
        {
            m_bullets = new BulletController[maxBullets];
            for (int i = 0; i < maxBullets; ++i)
            {
                m_bullets[i] = Instantiate(m_bulletPrefab, transform.position, Quaternion.identity);
            }

            if (autoAim)
            {
                if (m_autoTargetSelector == null)
                {
                    m_autoTargetSelector = new AutoTargetSelector(owner.gameObject);
                }

                m_autoTargetSelector.DetectTargets();
                m_autoTargetSelector.SetupTargetDetection();
            }
        }

        void OnDestroy()
        {
            if (autoAim)
            {
                m_autoTargetSelector.DisableTargetDetection();
            }
        }

        private void Update()
        {
            FireBullets();

            if (autoAim)
            {
                var target = m_autoTargetSelector.SelectTarget()?.target.transform;

                if (target != null)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    float angle = (Mathf.Atan2(direction.y, direction.x) - Mathf.PI * 0.5f) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, trackingSpeed * Time.deltaTime);

                    // start shooting if target withing the spray cone
                    Vector3 left = Quaternion.Euler(0, 0, bulletSprayCone) * transform.up;
                    Vector3 right = Quaternion.Euler(0, 0, -bulletSprayCone) * transform.up;
                    Vector3 targetDirection = target.position - bulletSpawnPoint.position;
                    m_isShooting = Vector3.Cross(left, targetDirection).z < 0 && Vector3.Cross(-right, targetDirection).z < 0;
                }
                else
                {
                    m_isShooting = false;
                }

                if (target != null && target.gameObject.scene.IsValid())
                {
                    float angle = Utility.AngleTo(bulletSpawnPoint.transform.position, target.transform.position) - 90f;
                    bulletSpawnPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        public void EnemyShoot()
        {
            int nextBullet = m_bulletIndex;
            m_bulletIndex = (m_bulletIndex + 1) % maxBullets;
            Vector3 bulletDirection = (m_playerTransform.position - transform.position).normalized;
            m_bullets[nextBullet].OnShoot(owner, bulletSpawnPoint.position, bulletDirection, sprite);
        }

        public void Shoot()
        {
            int nextBullet = m_bulletIndex;
            m_bulletIndex = (m_bulletIndex + 1) % maxBullets;

            float angle = Random.Range(-bulletSprayCone, bulletSprayCone);
            Vector3 bulletDirection = Quaternion.Euler(0, 0, angle) * bulletSpawnPoint.transform.rotation * Vector3.up;
            bulletDirection.Normalize();

            m_bullets[nextBullet].OnShoot(owner, bulletSpawnPoint.position, bulletDirection, sprite);
        }

        void FireBullets()
        {
            if (!m_isShooting)
            {
                return;
            }

            m_canShoot = m_shootDelay <= 0;
            m_shootDelay -= Time.deltaTime;

            if (!m_canShoot)
            {
                return;
            }

            m_shootDelay = shootDelay;
            Shoot();
        }

        void OnDrawGizmos()
        {
            const float rayLength = 3f;
            Vector3 left = Quaternion.Euler(0, 0, bulletSprayCone) * transform.up;
            Vector3 right = Quaternion.Euler(0, 0, -bulletSprayCone) * transform.up;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(bulletSpawnPoint.position, rayLength * left);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(bulletSpawnPoint.position, rayLength * right);
        }
    }
}