using UnityEngine;
using UnityEngine.InputSystem;

namespace LD57
{
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("How quickly the player moves.")]
        public float moveSpeed = 1;
        [Tooltip("How quickly the player strafes.")]
        public float strafeSpeed = 1;
        [Tooltip("How quickly the player rotates.")]
        public float rotationSpeed = 1;

        public Transform gun;

        private PlayerAnimationController m_animationControl;
        private Sprite m_playerBulletSprite;
        private PlayerInput m_playerInput;
        private InputAction m_moveAction;
        private InputAction m_lookAction;
        private InputAction m_attackAction;
        private Vector3 m_moveDirection;
        private Vector2 m_moveInput;
        private float m_rotationDirection;
        private BulletController[] m_bullets;
        private int m_bulletIndex = 0;
        private AimController m_aimController;

        void Awake()
        {
            m_animationControl = GetComponent<PlayerAnimationController>();
            m_aimController = GetComponent<AimController>();
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
            m_lookAction = m_playerInput.actions["Look"];
            m_attackAction = m_playerInput.actions["Attack"];
            m_playerBulletSprite = Resources.Load<Sprite>("Sprites/player_bullet");
            Cursor.visible = false;
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
            m_animationControl.DoShootingAnimation(true);
            m_aimController.StartShooting();
        }
        
        void StopShootingWeapon(InputAction.CallbackContext context)
        {
            m_animationControl.DoShootingAnimation(false);
            m_aimController.StopShooting();
        }

        void OnDrawGizmos()
        {
            // draw player facing direction
            const float rayLength = 3f;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rayLength * transform.up);
        }
    }
}