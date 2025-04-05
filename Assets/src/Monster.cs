using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Monster : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string TargetableTag = "Targetable";

    public float MovementSpeed = 2.5f;
    public float RotationSpeed = 10000f;
    public float Damage = 20.0f;
    public float DamageCooldown = 1;
    public float AttackRange = 1;

    private List<Killable> Targetables = new();
    private float lastAttackTime = float.MinValue;

    void Start()
    {
        var killables = GameObject.FindObjectsByType<Killable>(FindObjectsSortMode.None);
        foreach (var killable in killables)
        {
            if (killable.team != Team.Neutral && killable.team != GetComponent<Killable>().team)
            {
                AddTarget(killable);
            }
        }
    }

    void OnDestroy()
    {
        if (Targetables.Count > 0)
        {
            Targetables.ForEach(x => x.OnKilled.RemoveListener(RemoveTarget));
        }
    }

    void Update()
    {
        var targetableGameObjects = SelectTarget();
        if (targetableGameObjects == null)
        {
            return;
        }

        if (CanAttackTarget(targetableGameObjects))
        {
            AttackTarget(targetableGameObjects);
        }
        else
        {
            MoveTowardsTarget(targetableGameObjects);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.ConvertTo<Killable>())
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.ConvertTo<Killable>())
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }

    protected bool CanAttackTarget(KillableTarget killableTarget)
    {
        return killableTarget.Distance < AttackRange;
    }

    protected void AttackTarget(KillableTarget killableTarget)
    {
        DamageTarget(killableTarget.Target);
    }

    protected void MoveTowardsTarget(KillableTarget killableTarget)
    {
        var target = killableTarget.Target;
        float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        GetComponent<Rigidbody2D>().MovePositionAndRotation(
            Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * MovementSpeed),
            Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * RotationSpeed));
    }

    private KillableTarget SelectTarget()
    {
        if (Targetables.Count == 0)
        {
            return null;
        }

        var targetables = Targetables.Select(target => new KillableTarget
        {
            Target = target,
            Distance = Vector3.Distance(target.transform.position, transform.position)
        });

        return targetables.Aggregate((currentMin, targetable) => SelectPriorityTarget(currentMin, targetable));
    }

    private KillableTarget SelectPriorityTarget(KillableTarget currentTarget, KillableTarget newPotentialTarget)
    {
        if (currentTarget == null)
        {
            return newPotentialTarget;
        }

        return newPotentialTarget.PriorityDistance < currentTarget.PriorityDistance ? newPotentialTarget : currentTarget;
    }

    public void RemoveTarget(Killable killable)
    {
        Targetables.Remove(killable);
    }

    public void AddTarget(Killable target)
    {
        Targetables.Add(target);
        target.OnKilled.AddListener(RemoveTarget);
    }

    private void DamageTarget(Killable target)
    {
        if (lastAttackTime + DamageCooldown > Time.time)
        {
            return;
        }

        target.Damage(GetComponent<Killable>(), Damage);
        lastAttackTime = Time.time;
    }

    protected class KillableTarget
    {
        public Killable Target;
        public float Distance;
        public float PriorityTargettingRatio;

        public float PriorityDistance => Distance / Target.PriorityTargettingRatio;
    }
}
