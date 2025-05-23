using UnityEngine;

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
        public float maxHealth = 100.0f;
        public float health = 100.0f;
        public float priorityTargettingRatio = 1.0f;
        public Team team = Team.Neutral;

        private KillableEventHandler eventHandler;

        public void Start()
        {
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<KillableEventHandler>();
            eventHandler?.NotifyOnSpawned(this);
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
            if (damager.team == team || health <= 0)
            {
                return false;
            }

            health -= damageAmount;
            eventHandler?.NotifyOnDamaged(this);

            if (health <= 0)
            {
                eventHandler?.NotifyOnKilled(this);
                Destroy(gameObject);
            }

            return true;
        }
    }
}