using UnityEngine;
using UnityEngine.Events;

public enum Team
{
    Neutral,
    Team1,
    Team2
}

public class Killable : MonoBehaviour
{
    public UnityEvent<Killable> OnKilled;
    public float MaxHealth = 100.0f;
    public float Health = 100.0f;
    public float PriorityTargettingRatio = 1.0f;
    public Team team = Team.Neutral;

    public void Start()
    {
        if (OnKilled == null)
        {
            OnKilled = new UnityEvent<Killable>();
        }
    }

    public void Heal(Killable healer, float healAmount)
    {
        if (healer.team == team)
        {
            Health = Mathf.Min(Health + healAmount, MaxHealth);
        }
    }

    public void Damage(Killable damager, float damageAmount)
    {
        if (damager.team == team)
        {
            // No friendly fire
            return;
        }

        Health -= damageAmount;

        if (Health <= 0)
        {
            OnKilled?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
