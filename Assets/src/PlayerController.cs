using UnityEngine;
using UnityEngine.InputSystem;

namespace LD57
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1;
        public float strafeSpeed = 1;
        public float rotationSpeed = 1;
        public int maxBullets = 10;
        public float shootDelay = 0.3f;
        [Tooltip("Controls the spread of the bullets when fired.")]
        public float bulletSprayCone = 15f;
        public BulletController bulletPrefab;
        public Transform bulletsParent;
        public Transform gun;

        private PlayerAnimationController m_animationControl;
        private PlayerInput m_playerInput;
        private InputAction m_moveAction;
        private InputAction m_lookAction;
        private InputAction m_attackAction;
        private Vector3 m_moveDirection;
        private Vector2 m_moveInput;
        private float m_rotationDirection;
        private BulletController[] m_bullets;
        private int m_bulletIndex = 0;
        private float m_shootDelay = 0;
        private bool m_canShoot;
        private bool m_isShooting;
        private Sprite m_playerBulletSprite;

        void Awake()
        {
            m_animationControl = GetComponent<PlayerAnimationController>();
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
            m_lookAction = m_playerInput.actions["Look"];
            m_attackAction = m_playerInput.actions["Attack"];
            m_playerBulletSprite = Resources.Load<Sprite>("Sprites/player_bullet");
            Cursor.visible = false;
        }

        void Start()
        {
            m_bullets = new BulletController[maxBullets];
            for(int i = 0; i < maxBullets; ++i)
            {
                m_bullets[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity, bulletsParent);
            }
        }

        void OnEnable()
        {
            m_moveAction.performed += MovePlayer;
            m_moveAction.canceled += StopPlayer;
            m_lookAction.started += OrientPlayerTowardsTarget;
            m_lookAction.canceled += StopPlayerRotation;
            m_attackAction.started += ShootWeapon;
            m_attackAction.canceled += StopShootingWeapon;
        }

        void OnDisable()
        {
            m_moveAction.performed -= MovePlayer;
            m_moveAction.canceled -= StopPlayer;
            m_lookAction.started -= OrientPlayerTowardsTarget;
            m_lookAction.canceled -= StopPlayerRotation;
            m_attackAction.started -= ShootWeapon;
            m_attackAction.canceled -= StopShootingWeapon;
        }

        void Update()
        {
            FireBullets();

            //transform.rotation *= Quaternion.Euler(0, 0, -rotationSpeed * Time.deltaTime * m_rotationDirection);
            m_moveDirection = transform.rotation * Vector3.up;

            //float strafeOrientation = Mathf.Sign(Vector3.Dot(transform.up, Vector3.up));
            //Vector3 strafe = strafeOrientation * m_moveInput.x * transform.right;
            Vector3 moveDirection = new(m_moveInput.x, m_moveInput.y, 0);

            if (moveDirection.magnitude != 0)
            {
                transform.position += Time.deltaTime * (moveSpeed * moveDirection);// + strafeSpeed * strafe);

                m_animationControl.FaceDirection(moveDirection.x);
            }
        }

        void MovePlayer(InputAction.CallbackContext context)
        {
            m_moveInput = context.ReadValue<Vector2>();
        }

        void StopPlayer(InputAction.CallbackContext context)
        {
            m_moveInput.x = 0;
            m_moveInput.y = 0;
        }

        void OrientPlayerTowardsTarget(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            m_rotationDirection = input.x;
            //Vector3 targetDirection = new Vector3(input.x, input.y, 0).normalized;
            //if (targetDirection != Vector3.zero)
            //{
            //    Quaternion targetRotation = Quaternion.LookRotation(transform.forward, targetDirection);
            //    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime * rotationSpeed);
            //    m_moveDirection = transform.rotation * Vector3.up;
            //}
        }

        void StopPlayerRotation(InputAction.CallbackContext context)
        {
            m_rotationDirection = 0;
        }

        void ShootWeapon(InputAction.CallbackContext context)
        {
            m_isShooting = true;
            m_animationControl.DoShootingAnimation(true);
        }
        
        void StopShootingWeapon(InputAction.CallbackContext context)
        {
            m_isShooting = false;
            m_animationControl.DoShootingAnimation(false);
        }

        void FireBullets()
        {
            if(!m_isShooting)
            {
                return;
            }

            m_canShoot = m_shootDelay <= 0;
            m_shootDelay -= Time.deltaTime;

            if(!m_canShoot)
            {
                return;
            }

            m_shootDelay = shootDelay;
            int nextBullet = m_bulletIndex;
            m_bulletIndex = (m_bulletIndex + 1) % maxBullets;

            float angle = Random.Range(-bulletSprayCone, bulletSprayCone);
            Vector3 bulletDirection = Quaternion.Euler(0, 0, angle) * transform.up;
            bulletDirection.Normalize();
            
            m_bullets[nextBullet].OnShoot(gun.position, bulletDirection, m_playerBulletSprite);
        }

        void OnDrawGizmos()
        {
            // draw player facing direction
            const float rayLength = 3f;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rayLength * transform.up);

            // draw bullet cone
            Gizmos.color = Color.red;
            Vector3 left = Quaternion.Euler(0, 0, bulletSprayCone) * transform.up;
            Vector3 right = Quaternion.Euler(0, 0, -bulletSprayCone) * transform.up;
            Gizmos.DrawRay(gun.position, rayLength * left);
            Gizmos.DrawRay(gun.position, rayLength * right);
        }
    }
}