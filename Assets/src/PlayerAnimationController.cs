namespace LD57
{
    using UnityEngine;

    public class PlayerAnimationController : MonoBehaviour
    {
        public SpriteRenderer player_sprite;
        public GameObject player_hand;
        public GameObject player_gun;

        private bool m_shoot = false;
        private Sprite[] gun;
        private SpriteRenderer m_gun_sprite;
        private Vector3 m_hand_offset = new(0.32f, -0.1f, 0);

        public void FaceDirection(float dir)
        {
            if (Mathf.Sign(dir) == 0)
            {
                return;
            }

            var flip = Mathf.Sign(dir) == -1f;

            player_sprite.flipX = flip;
            m_gun_sprite.flipX = flip;
        }

        public void DoShootingAnimation(bool shoot)
        {
            m_shoot = shoot;
            m_gun_sprite.sprite = gun[shoot ? 1 : 0];
        }

        private void Awake()
        {
            m_gun_sprite = player_gun.GetComponent<SpriteRenderer>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            gun = Resources.LoadAll<Sprite>("Sprites/gun_scythe");
        }

        // Update is called once per frame
        void Update()
        {
            var flipped = player_sprite.flipX ? -1 : 1;
            player_hand.transform.position = transform.position;
            player_hand.transform.Translate(m_hand_offset.x * flipped, m_hand_offset.y, 0);

            if (m_shoot)
            {
                var mouse_world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float angleRad = Mathf.Atan2(mouse_world_pos.y - player_gun.transform.position.y, mouse_world_pos.x - player_gun.transform.position.x);
                float angleDeg = (180 / Mathf.PI * angleRad) - 90;
                player_gun.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
            }
            else
            {
                player_gun.transform.rotation = Quaternion.identity;
            }
        }
    }
}