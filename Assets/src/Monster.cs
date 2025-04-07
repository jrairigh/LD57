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
        private Coin m_coinPrefab;
        private NavMeshAgent m_agent = null;
        private AutoTargetSelector m_autoTargetSelector = null;

        public void DropLoot()
        {
            Coin[] coins = new Coin[purseSize];
            for(int i = 0; i < purseSize; ++i)
            {
                coins[i] = Instantiate(m_coinPrefab, transform.position, Quaternion.identity);
                coins[i].gameObject.SetActive(true);
            }

            foreach (var coin in coins)
            {
                Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
                Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                rb.AddForce(coinExplosionForce * force, ForceMode2D.Impulse);
            }
        }

        void Awake()
        {
            m_coinPrefab = Resources.Load<Coin>("Prefabs/Coin");
        }

        void Start()
        {
            m_agent = GetComponent<NavMeshAgent>();
            m_agent.updateRotation = false;
            m_agent.updateUpAxis = false;

            if (m_autoTargetSelector == null)
            {
                m_autoTargetSelector = new AutoTargetSelector(gameObject);
            }

            m_autoTargetSelector.onKilled.AddListener(KillableDied);
            m_autoTargetSelector.DetectTargets();
            m_autoTargetSelector.SetupTargetDetection();
        }
        void OnDestroy()
        {
            m_autoTargetSelector.DisableTargetDetection();
        }

        protected virtual void Update()
        {
            var target = m_autoTargetSelector.SelectTarget();
            if (target == null)
            {
                return;
            }

            if (CanAttackTarget(target))
            {
                AttackTarget(target);
            }
            else
            {
                MoveTowardsTarget(target);
            }
        }

        protected virtual bool CanAttackTarget(KillableTarget killableTarget)
        {
            return killableTarget.distance <= attackRange;
        }

        protected abstract void AttackTarget(KillableTarget killableTarget);

        protected virtual void MoveTowardsTarget(KillableTarget killableTarget)
        {
            var target = killableTarget.target;
            float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            m_agent.SetDestination(target.transform.position);
        }

        public void KillableDied(Killable dead)
        {
            if (dead == GetComponent<Killable>())
            {
                DropLoot();
            }
        }

        protected void DamageKillable(Killable target)
        {
            target.TryDamage(GetComponent<Killable>(), attackDamage);
        }
    }
}