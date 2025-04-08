using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.Events;

namespace LD57
{
    public class KillableEventHandler : MonoBehaviour
    {
        public UnityEvent<Killable> onSpawned;
        public UnityEvent<Killable> onKilled;
        public UnityEvent<Killable> onDamaged;

        private NavMeshSurface navMeshSurface;

        void Awake()
        {
            onKilled ??= new UnityEvent<Killable>();
            onSpawned ??= new UnityEvent<Killable>();

            navMeshSurface = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshSurface>();
        }

        public void NotifyOnSpawned(Killable killableSpawned)
        {
            onSpawned.Invoke(killableSpawned);
            navMeshSurface.BuildNavMesh();
        }

        public void NotifyOnKilled(Killable killableKilled)
        {
            onKilled.Invoke(killableKilled);
            navMeshSurface.BuildNavMeshAsync();
        }
        public void NotifyOnDamaged(Killable killableKilled)
        {
            onDamaged.Invoke(killableKilled);
        }
    }
}