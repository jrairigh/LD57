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
        [Tooltip("How much money the player has.")]
        public int money = 100;
        public AimController aimController;
        public GameObject turretPrefab;
        PlayerInput m_playerInput;
        InputAction m_moveAction;
        InputAction m_attackAction;
        InputAction m_interactAction;
        Vector2 m_moveInput;
        PlayerAnimationController m_animationControl;
        SpriteRenderer m_playerSprite;
        bool m_shooting;
        Camera m_camera;
        
        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log($"Player gained ${amount}, total in purse ${money}");
        }
        
        public void RemoveMoney(int amount)
        {
            money -= amount;
            Debug.Log($"Player pays ${amount}, total in purse ${money}");
        }

        void Awake()
        {
            m_animationControl = GetComponent<PlayerAnimationController>();
            m_playerInput = GetComponent<PlayerInput>();
            m_playerSprite = GetComponent<SpriteRenderer>();
            m_camera = Camera.main;
            m_moveAction = m_playerInput.actions["Move"];
            m_attackAction = m_playerInput.actions["Attack"];
            m_interactAction = m_playerInput.actions["Interact"];
            Cursor.visible = false;
        }

        void OnEnable()
        {
            m_moveAction.performed += MovePlayer;
            m_moveAction.canceled += StopPlayer;
            m_attackAction.started += ShootWeapon;
            m_attackAction.canceled += StopShootingWeapon;
            m_interactAction.performed += PlaceItem;
        }

        void OnDisable()
        {
            m_moveAction.performed -= MovePlayer;
            m_moveAction.canceled -= StopPlayer;
            m_attackAction.started -= ShootWeapon;
            m_attackAction.canceled -= StopShootingWeapon;
            m_interactAction.performed -= PlaceItem;
        }

        void Update()
        {
            Vector3 moveDirection = new(m_moveInput.x, m_moveInput.y, 0);

            if (moveDirection.magnitude != 0)
            {
                transform.position += Time.deltaTime * (moveSpeed * moveDirection);

                if (!m_shooting)
                {
                    m_animationControl.FaceDirection(moveDirection.x);
                }
            }

            if (m_shooting)
            {
                var mouseWorldPos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                var angleToMouse = Mathf.Rad2Deg * Mathf.Atan2(mouseWorldPos.y - transform.position.y, mouseWorldPos.x - transform.position.x);
                var angleDifference = Mathf.Abs(Mathf.DeltaAngle(m_playerSprite.flipX ? 180 : 0, angleToMouse));

                if (!m_playerSprite.flipX)
                {
                    m_animationControl.FaceDirection(angleDifference >= 90 ? -1 : 1);
                }
                else
                {
                    m_animationControl.FaceDirection(angleDifference > 90 ? 1 : -1);
                }
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

        void ShootWeapon(InputAction.CallbackContext context)
        {
            m_shooting = true;
            m_animationControl.DoShootingAnimation(true);
            aimController.StartShooting();
        }
        
        void StopShootingWeapon(InputAction.CallbackContext context)
        {
            m_shooting = false;
            m_animationControl.DoShootingAnimation(false);
            aimController.StopShooting();
        }

        void PlaceItem(InputAction.CallbackContext context)
        {
            if(money < 10)
            {
                Debug.Log("Not enough money to place turret!");
                return;
            }

            Debug.Log($"Turret cost $10, total in purse ${money}");
            money -= 10;

            float faceDirection = !m_playerSprite.flipX ? 1 : -1;
            Vector3 position = transform.position + faceDirection * transform.right;
            GameObject turret = Instantiate(turretPrefab, position, Quaternion.identity);
            turret.SetActive(true);
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