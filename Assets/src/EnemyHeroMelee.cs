using UnityEngine;

namespace LD57
{
    public class EnemyHeroMelee : Monster
    {
        private const string idleAnimation = "MeroMeleeIdle";
        private const string attackAnimation = "MeroMeleeAttack";

        private SpriteRenderer sprite;
        private KillableTarget target;
        private Animator attackAnimator;
        private Vector3 lastPosition;

        private string currentAnimation => attackAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        protected void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            attackAnimator = GetComponent<Animator>();
            attackAnimator.Play(idleAnimation);
        }

        protected override void Update()
        {
            base.Update();
            
            var xDifference = transform.position.x - lastPosition.x;

            if (xDifference != 0)
            {
                sprite.flipX = xDifference < 0;
            }

            lastPosition = transform.position;
        }

        public void DoDamage()
        {
            if (target is null)
            {
                return;
            }

            DamageKillable(target.target);

            target = null;
        }

        protected override void AttackTarget(KillableTarget killableTarget)
        {
            if (currentAnimation != attackAnimation)
            {
                attackAnimator.Play(attackAnimation);
                //PauseAgent(true);
                target = killableTarget;
            }
        }

        private void AttackAnimationEnd()
        {
            attackAnimator.Play(idleAnimation);
            //PauseAgent(false);
        }
    }
}