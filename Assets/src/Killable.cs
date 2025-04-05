using UnityEngine;
using UnityEngine.Events;

public class Killable : MonoBehaviour
{
    public UnityEvent OnKilled = new();
    public float MaxHealth = 100.0f;
    public float Health = 100.0f;

    public void Heal(float healAmount)
    {
        Health = Mathf.Min(Health + healAmount, MaxHealth);
    }

    public void Damage(float damageAmount)
    {
        Health -= damageAmount;

        if (Health < 0)
        {
            OnKilled?.Invoke();
            Destroy(gameObject);
        }
    }
}
