using UnityEngine;

namespace LD57
{
    public class Coin : MonoBehaviour
    {
        [Tooltip("How much money this coin is worth.")]
        public int value = 1;
        [Tooltip("How long the coin will exist before being destroyed.")]
        public float lifetime = 60f;

        void Update()
        {
            lifetime -= Time.deltaTime;
            if(lifetime <= 0)
            {
                Destroy(gameObject);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Player"))
            {
                return;
            }

            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddMoney(value);
                Destroy(gameObject);
            }
        }
    }
}