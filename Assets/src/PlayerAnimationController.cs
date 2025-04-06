namespace LD57
{
    using UnityEngine;

    public class PlayerAnimationController : MonoBehaviour
    {
        private Sprite[] gun;
        private SpriteRenderer m_player_sprite;
        private SpriteRenderer m_gun_sprite;
        private GameObject m_player_hand;
        private Vector3 m_hand_offset = new(0.32f, -0.1f, 0);

        public void FaceDirection(float dir)
        {
            if (Mathf.Sign(dir) == 0)
            {
                return;
            }

            var flip = Mathf.Sign(dir) == -1f;

            m_player_sprite.flipX = flip;
            m_gun_sprite.flipX = flip;
        }

        public void DoShootingAnimation(bool shoot)
        {
            m_gun_sprite.sprite = gun[shoot ? 1 : 0];
        }

        private void Awake()
        {
            m_player_sprite = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
            m_player_hand = transform.GetChild(2).gameObject;
            m_gun_sprite = m_player_hand.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            gun = Resources.LoadAll<Sprite>("Sprites/gun_scythe");
        }

        // Update is called once per frame
        void Update()
        {
            var flipped = m_player_sprite.flipX ? -1 : 1;
            m_player_hand.transform.position = transform.position;
            m_player_hand.transform.Translate(m_hand_offset.x * flipped, m_hand_offset.y, 0);
        }
    }
}