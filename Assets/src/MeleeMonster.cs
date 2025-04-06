namespace LD57
{
    public class MeleeMonster : Monster
    {
        protected override void AttackTarget(KillableTarget killableTarget)
        {
            DamageKillable(killableTarget.target);
        }
    }
}