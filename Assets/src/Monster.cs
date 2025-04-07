using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace LD57
{
    public abstract class Monster : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const string TargetableTag = "Targetable";

        public float movementSpeed = 2.5f;
        public float rotationSpeed = 10000f;
        public float attackDamage = 20.0f;
        public float attackCooldown = 1;
        public float attackRange = 1;

        [Tooltip("How much coins the monster holds")]
        public int purseSize = 10;
        [Tooltip("How much force to apply to the coins when they are dropped")]
        public float coinExplosionForce = 10.0f;
        public Coin coinPrefab;
        
        private List<KillableTarget> m_targetables = new();
        private KillableTarget m_primaryTarget = null;
        private KillableTarget m_lastPrimaryTarget = null;
        private float m_lastAttackTime = float.MinValue;
        private NavMeshAgent agent = null;
        private NavMeshPath path = null;
        private KillableEventHandler killableEventHandler = null;

        public void DropLoot()
        {
            Coin[] coins = new Coin[purseSize];
            for(int i = 0; i < purseSize; ++i)
            {
                coins[i] = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                coins[i].gameObject.SetActive(true);
            }

            foreach (var coin in coins)
            {
                Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
                Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                rb.AddForce(coinExplosionForce * force, ForceMode2D.Impulse);
            }
        }

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            path = new NavMeshPath();

            var killables = GameObject.FindObjectsByType<Killable>(FindObjectsSortMode.None);
            foreach (var killable in killables)
            {
                if (killable.team != Team.Neutral)
                {
                    AddTarget(killable);
                }
            }

            killableEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<KillableEventHandler>();
            killableEventHandler.onSpawned.AddListener(AddTarget);
            killableEventHandler.onKilled.AddListener(RemoveTarget);
        }

        void OnDestroy()
        {
            killableEventHandler.onSpawned.RemoveListener(AddTarget);
            killableEventHandler.onKilled.RemoveListener(RemoveTarget);
        }

        protected virtual void Update()
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

            if (m_lastPrimaryTarget?.target != m_primaryTarget?.target)
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

            if (m_lastPrimaryTarget?.target != m_primaryTarget?.target || m_primaryTarget?.target == collisionKillable)
            {
                agent.isStopped = false;
                m_lastPrimaryTarget = null;
            }
        }

        protected virtual bool CanAttackTarget(KillableTarget killableTarget)
        {
            return killableTarget.distance <= attackRange;
        }

        protected abstract void AttackTarget(KillableTarget killableTarget);

        protected virtual void MoveTowardsTarget(KillableTarget killableTarget)
        {
            if (agent.enabled)
            {
                var target = killableTarget.target;
                agent.SetDestination(target.transform.position);
            }
        }

        protected void PauseAgent(bool pause)
        {
            agent.enabled = !pause;
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
            path ??= new NavMeshPath();
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
            if (killable == GetComponent<Killable>())
            {
                DropLoot();
            }
            else
            {
                m_targetables.RemoveAll(x => x.target == killable);
            }
        }

        public void AddTarget(Killable target)
        {
            if (target.team == GetComponent<Killable>().team)
            {
                return;
            }

            var killableTarget = new KillableTarget
            {
                target = target,
                distance = float.MaxValue
            };
            UpdateDistanceToTarget(killableTarget);
            m_targetables.Add(killableTarget);
        }

        protected void DamageKillable(Killable target)
        {
            target.TryDamage(GetComponent<Killable>(), attackDamage);
            m_lastAttackTime = Time.time;
        }

        protected class KillableTarget
        {
            public Killable target;
            public float distance;

            public float PriorityDistance => distance / target.priorityTargettingRatio;
        }
    }
}