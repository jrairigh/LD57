using UnityEngine;

namespace LD57
{
    public class BulletController : MonoBehaviour
    {
        public float speed = 10f;
        public float lifetime = 2f;
        private Vector2 m_direction;
        private float m_lifetime;

        public void OnShoot(Vector2 position, Quaternion rotation, Vector2 direction)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = rotation;
            m_direction = direction.normalized;
            m_lifetime = lifetime;
        }

        private void Update()
        {
            transform.Translate(speed * Time.deltaTime * m_direction);
            gameObject.SetActive(m_lifetime > 0);
            m_lifetime -= Time.deltaTime;
        }
    }
}