using UnityEngine;

namespace LD57
{
    public class BulletController : MonoBehaviour
    {
        public float speed = 10f;
        public float lifetime = 2f;

        private SpriteRenderer m_sprite;
        private Vector2 m_direction;
        private float m_lifetime;

        public void OnShoot(Vector2 position, Vector2 direction, Sprite sprite)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            m_direction = direction.normalized;
            m_lifetime = lifetime;

            m_sprite.sprite = sprite;
        }

        private void Awake()
        {
            m_sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            transform.Translate(speed * Time.deltaTime * m_direction);
            gameObject.SetActive(m_lifetime > 0);
            m_lifetime -= Time.deltaTime;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Bullet hit: " + collision.gameObject.name);
            m_lifetime = 0;

            // tell objects that it was hit by a bullet
        }
    }
}