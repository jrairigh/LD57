using UnityEngine;

namespace LD57
{
    public class BulletController : MonoBehaviour
    {
        [Tooltip("How quickly the bullet moves.")]
        public float speed = 10f;
        [Tooltip("How long the bullet lasts before it is destroyed.")]
        public float lifetime = 2f;
        [Tooltip("How much damage the bullet does.")]
        public float bulletDamage = 10f;
        public GameObject spriteGameObject;
        private Vector2 m_direction;
        private float m_lifetime;
        private Killable m_owner;

        public void OnShoot(Killable owner, Vector2 position, Vector2 direction, Sprite sprite)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = Quaternion.identity;
            spriteGameObject.transform.rotation = Quaternion.FromToRotation(Vector2.zero, direction);
            m_direction = direction.normalized;
            m_lifetime = lifetime;
            m_owner = owner;

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
            m_lifetime = 0;

            var killable = collision.collider.GetComponent<Killable>();
            if(killable != null && m_owner != null)
            {
                killable.Damage(m_owner, bulletDamage);
            }
        }
    }
}