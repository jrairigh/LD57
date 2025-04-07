using UnityEngine;

namespace LD57
{
    public class EnemyHeroRanger : Monster
    {
        private const string idleAnimation = "HeroRangerIdle";
        private const string attackAnimation = "HeroRangerAttack";

        private SpriteRenderer sprite;
        private Animator attackAnimator;
        private Transform bulletSpawn;
        private AimController aimController;
        private Vector3 lastPosition;

        private string currentAnimation => attackAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        new void Awake()
        {
            base.Awake();

            sprite = GetComponent<SpriteRenderer>();
            attackAnimator = GetComponent<Animator>();
            bulletSpawn = transform.GetChild(0);
            aimController = bulletSpawn.GetComponent<AimController>();
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
            aimController.Shoot();
        }

        protected override void AttackTarget(KillableTarget killableTarget)
        {
            if (currentAnimation != attackAnimation)
            {
                attackAnimator.Play(attackAnimation);
            }
        }

        private void AttackAnimationEnd()
        {
            attackAnimator.Play(idleAnimation);
        }
    }
}