using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string TargetableTag = "Targetable";

    public float movementSpeed = 2.5f;
    public float rotationSpeed = 10000f;
    public float attackDamage = 20.0f;
    public float attackCooldown = 1;
    public float attackRange = 1;

    private List<Killable> m_targetables = new();
    private KillableTarget m_primaryTarget = null;
    private KillableTarget m_lastPrimaryTarget = null;
    private float m_lastAttackTime = float.MinValue;

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
        if (m_targetables.Count > 0)
        {
            m_targetables.ForEach(x => x.onKilled.RemoveListener(RemoveTarget));
        }
    }

    void Update()
    {
        m_primaryTarget = SelectTarget();
        if (m_primaryTarget == null)
        {
            return;
        }

        if (CanAttackTarget(m_primaryTarget))
        {
            AttackTarget(m_primaryTarget);
        }
        else
        {
            MoveTowardsTarget(m_primaryTarget);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var collisionKillable = collision.gameObject.GetComponent<Killable>();
        if (collisionKillable == null)
        {
            return;
        }

       if (m_primaryTarget?.target == collisionKillable)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            m_lastPrimaryTarget = m_primaryTarget;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var collisionKillable = collision.gameObject.GetComponent<Killable>();
        if (collisionKillable == null)
        {
            return;
        }

        if (m_lastPrimaryTarget.target != m_primaryTarget.target)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            m_lastPrimaryTarget = null;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        var collisionKillable = collision.gameObject.GetComponent<Killable>();
        if (collisionKillable == null)
        {
            return;
        }

        if (m_lastPrimaryTarget?.target != m_primaryTarget.target || m_primaryTarget?.target == collisionKillable)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            m_lastPrimaryTarget = null;
        }
    }

    protected virtual bool CanAttackTarget(KillableTarget killableTarget)
    {
        return killableTarget.distance <= attackRange && m_lastAttackTime + attackCooldown <= Time.time;
    }

    protected abstract void AttackTarget(KillableTarget killableTarget);

    protected virtual void MoveTowardsTarget(KillableTarget killableTarget)
    {
        var target = killableTarget.target;
        float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        GetComponent<Rigidbody2D>().MovePositionAndRotation(
            Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * movementSpeed),
            Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed));
    }

    private KillableTarget SelectTarget()
    {
        if (m_targetables.Count == 0)
        {
            return null;
        }

        var targetables = m_targetables.Select(target => new KillableTarget
        {
            target = target,
            distance = Vector3.Distance(target.transform.position, transform.position)
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
        m_targetables.Remove(killable);
    }

    public void AddTarget(Killable target)
    {
        m_targetables.Add(target);
        target.onKilled.AddListener(RemoveTarget);
    }

    protected void DamageKillable(Killable target)
    {
        if (m_lastAttackTime + attackCooldown > Time.time)
        {
            return;
        }

        target.Damage(GetComponent<Killable>(), attackDamage);
        m_lastAttackTime = Time.time;
    }

    protected class KillableTarget
    {
        public Killable target;
        public float distance;

        public float PriorityDistance => distance / target.priorityTargettingRatio;
    }
}
