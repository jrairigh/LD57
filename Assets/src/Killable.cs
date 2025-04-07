using UnityEngine;
using UnityEngine.Events;

namespace LD57
{
    public enum Team
    {
        Neutral,
        Team1,
        Team2
    }

    public class Killable : MonoBehaviour
    {
        public UnityEvent<Killable> onKilled;
        public float maxHealth = 100.0f;
        public float health = 100.0f;
        public float priorityTargettingRatio = 1.0f;
        public Team team = Team.Neutral;

        public void Start()
        {
            if (onKilled == null)
            {
                onKilled = new UnityEvent<Killable>();
            }
        }

        public void Heal(Killable healer, float healAmount)
        {
            if (healer.team == team)
            {
                health = Mathf.Min(health + healAmount, maxHealth);
            }
        }

        public bool TryDamage(Killable damager, float damageAmount)
        {
            if (damager.team == team)
            {
                // No friendly fire
                return false;
            }

            health -= damageAmount;

            if (health <= 0)
            {
                onKilled?.Invoke(this);
                Destroy(gameObject);
            }

            return true;
        }
    }
}