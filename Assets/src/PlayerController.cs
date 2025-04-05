using UnityEngine;
using UnityEngine.InputSystem;

namespace LD57
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1;
        public float strafeSpeed = 1;
        public float rotationSpeed = 1;
        PlayerInput m_playerInput;
        InputAction m_moveAction;
        InputAction m_lookAction;
        Vector3 m_moveDirection;
        Vector2 m_moveInput;

        void Awake()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
            m_lookAction = m_playerInput.actions["Look"];
            Cursor.visible = false;
        }

        void OnEnable()
        {
            m_moveAction.started += MovePlayer;
            m_moveAction.canceled += StopPlayer;
            m_lookAction.started += OrientPlayerTowardsTarget;
        }

        void Onsable()
        {
            m_moveAction.started -= MovePlayer;
            m_moveAction.canceled -= StopPlayer;
            m_lookAction.started -= OrientPlayerTowardsTarget;
        }

        void Update()
        {
            float strafeOrientation = Mathf.Sign(Vector3.Dot(transform.up, Vector3.up));
            Vector3 strafe = strafeOrientation * m_moveInput.x * transform.right;
            Vector3 moveDirection = m_moveDirection;
            moveDirection.x *= m_moveInput.y;
            moveDirection.y *= m_moveInput.y;
            moveDirection.z = 0;
            transform.position += Time.deltaTime * (moveSpeed * moveDirection + strafeSpeed * strafe);
        }

        void MovePlayer(InputAction.CallbackContext context)
        {
            m_moveInput += context.ReadValue<Vector2>();
        }

        void StopPlayer(InputAction.CallbackContext context)
        {
            m_moveInput.x = 0;
            m_moveInput.y = 0;
        }

        void OrientPlayerTowardsTarget(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 targetDirection = new Vector3(input.x, input.y, 0).normalized;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(transform.forward, targetDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime * rotationSpeed);
                m_moveDirection = transform.rotation * Vector3.up;
            }
        }

        void OnDrawGizmos()
        {
            const float rayLength = 3f;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rayLength * transform.up);
        }
    }
}