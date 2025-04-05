using UnityEngine;
using UnityEngine.InputSystem;

namespace LD57
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1;
        PlayerInput m_playerInput;
        InputAction m_moveAction;
        Vector3 m_moveDirection;

        void Awake()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
        }

        void OnEnable()
        {
            m_moveAction.started += MovePlayer;
            m_moveAction.canceled += StopPlayer;
        }

        void Onsable()
        {
            m_moveAction.started -= MovePlayer;
            m_moveAction.canceled -= StopPlayer;
        }

        void Update()
        {
            transform.position += moveSpeed * Time.deltaTime * m_moveDirection;
        }

        void MovePlayer(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Debug.Log($"Input: {input}");
            m_moveDirection.x = input.x;
            m_moveDirection.y = input.y;
            m_moveDirection.z = 0;
        }

        void StopPlayer(InputAction.CallbackContext context)
        {
            m_moveDirection.x = 0;
            m_moveDirection.y = 0;
            m_moveDirection.z = 0;
        }
    }
}