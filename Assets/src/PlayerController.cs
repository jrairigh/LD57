using UnityEngine;
using UnityEngine.InputSystem;

namespace LD57
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1;
        PlayerInput m_playerInput;
        InputAction m_moveAction;

        void Awake()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_moveAction = m_playerInput.actions["Move"];
        }

        void OnEnable()
        {
            m_moveAction.started += MovePlayer;
        }

        void Onsable()
        {
            m_moveAction.started -= MovePlayer;
        }

        void Update()
        {

        }

        void MovePlayer(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Debug.Log($"Input: {input}");
            Vector3 moveDirection = new Vector3(input.x, input.y, 0).normalized;
            transform.position += moveSpeed * Time.deltaTime * moveDirection;
        }
    }
}