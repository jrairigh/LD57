namespace LD57
{
    public class KillableTarget
    {
        public Killable target;
        public float distance;

        public float PriorityDistance => distance / target.priorityTargettingRatio;
    }
}