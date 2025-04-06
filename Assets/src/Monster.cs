using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Monster : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string TargetableTag = "Targetable";

    public float movementSpeed = 2.5f;
    public float rotationSpeed = 10000f;
    public float attackDamage = 20.0f;
    public float attackCooldown = 1;
    public float attackRange = 1;

    private List<KillableTarget> m_targetables = new();
    private KillableTarget m_primaryTarget = null;
    private KillableTarget m_lastPrimaryTarget = null;
    private float m_lastAttackTime = float.MinValue;
    private NavMeshAgent agent = null;
    private NavMeshPath path = null;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        path = new NavMeshPath();

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
            m_targetables.ForEach(x => x.target.onKilled.RemoveListener(RemoveTarget));
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
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
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
            agent.isStopped = false;
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
            agent.isStopped = false;
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
        GetComponent<Rigidbody2D>().SetRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed));
        agent.SetDestination(target.transform.position);
    }

    private KillableTarget SelectTarget()
    {
        if (m_targetables.Count == 0)
        {
            return null;
        }

        m_targetables.ForEach(x => UpdateDistanceToTarget(x));
        return m_targetables.Aggregate((currentMin, targetable) => SelectPriorityTarget(currentMin, targetable));
    }

    private void UpdateDistanceToTarget(KillableTarget killableTarget)
    {
        var target = killableTarget.target;
        NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, path);
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            float totalDistance = 0.0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            killableTarget.distance = totalDistance;
        }
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
        m_targetables.RemoveAll(x => x.target == killable);
    }

    public void AddTarget(Killable target)
    {
        var killableTarget = new KillableTarget
        {
            target = target,
            distance = float.MaxValue
        };
        UpdateDistanceToTarget(killableTarget);
        m_targetables.Add(killableTarget);
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
