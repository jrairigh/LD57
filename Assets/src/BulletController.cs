using UnityEngine;

namespace LD57
{
    public class BulletController : MonoBehaviour
    {
        [Tooltip("The damage the bullet does")]
        public float damage = 10f;
        [Tooltip("The speed of the bullet")]
        public float speed = 10f;
        [Tooltip("The life time of the bullet in seconds")]
        public float lifetime = 2f;
        [Tooltip("The game object containing the sprite")]
        public GameObject spriteGameObject;
        [Tooltip("The creator of the bullet")]
        public Killable creator;

        private Vector2 m_direction;
        private float m_lifetime;

        public void OnShoot(Vector2 position, Vector2 direction, Sprite sprite)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            spriteGameObject.transform.rotation = Quaternion.FromToRotation(Vector2.zero, direction);
            m_direction = direction.normalized;
            m_lifetime = lifetime;

            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
            spriteGameObject.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

            spriteGameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        void Update()
        {
            transform.Translate(speed * Time.deltaTime * m_direction);
            gameObject.SetActive(m_lifetime > 0);
            m_lifetime -= Time.deltaTime;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var victim = collision.gameObject;

            if(victim.TryGetComponent<Killable>(out var killable) && killable.TryDamage(creator, damage))
            {
                m_lifetime = 0;
            }            
        }
    }
}