using UnityEngine;
using UnityEngine.Events;

namespace LD57
{
    public class KillableEventHandler : MonoBehaviour
    {
        public UnityEvent<Killable> onSpawned;
        public UnityEvent<Killable> onKilled;

        void Awake()
        {
            onKilled ??= new UnityEvent<Killable>();
            onSpawned ??= new UnityEvent<Killable>();
        }
    }
}