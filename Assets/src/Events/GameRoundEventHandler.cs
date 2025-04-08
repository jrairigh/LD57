using System;
using UnityEngine;
using UnityEngine.Events;

namespace LD57
{
    public class GameRoundEventHandler : MonoBehaviour
    {
        public UnityEvent<int> onRoundStart;
        public UnityEvent<Killable> onPlayerHealthChanged;
        public UnityEvent<int> onCoinChanged;

        void Awake()
        {
            onRoundStart ??= new UnityEvent<int>();
            onPlayerHealthChanged ??= new UnityEvent<Killable>();
            onCoinChanged ??= new UnityEvent<int>();
        }

        public void NotifyOnRoundStart(int depth)
        {
            onRoundStart.Invoke(depth);
        }

        public void NotifyPlayerHealthChanged(Killable killable)
        {
            onPlayerHealthChanged.Invoke(killable);
        }

        public void NotifyCoinChanged(int count)
        {
            onCoinChanged.Invoke(count);
        }
    }
}