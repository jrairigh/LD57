using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace LD57
{
    public class AutoTargetSelector
    {
        public UnityEvent<Killable> onSpawned;
        public UnityEvent<Killable> onKilled;

        private List<KillableTarget> m_targetables = new();
        private NavMeshPath m_path = null;
        private readonly GameObject m_gameObject;
        private KillableEventHandler m_killableEventHandler = null;

        public AutoTargetSelector(GameObject gameObject)
        {
            m_gameObject = gameObject;

            onKilled = new UnityEvent<Killable>();
            onSpawned = new UnityEvent<Killable>();
        }

        public void DetectTargets()
        {
            var killables = GameObject.FindObjectsByType<Killable>(FindObjectsSortMode.None);
            foreach (var killable in killables)
            {
                if (killable.team != Team.Neutral && m_targetables.All(x => x.target != killable))
                {
                    AddTarget(killable);
                }
            }
        }

        public void SetupTargetDetection()
        {
            m_killableEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<KillableEventHandler>();
            m_killableEventHandler.onSpawned.AddListener(AddTarget);
            m_killableEventHandler.onKilled.AddListener(RemoveTarget);
        }

        public void DisableTargetDetection()
        {
            m_killableEventHandler.onSpawned.RemoveListener(AddTarget);
            m_killableEventHandler.onKilled.RemoveListener(RemoveTarget);
        }

        public KillableTarget SelectTarget()
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
            m_path ??= new NavMeshPath();
            NavMesh.CalculatePath(m_gameObject.transform.position, target.transform.position, NavMesh.AllAreas, m_path);
            if (m_path.status == NavMeshPathStatus.PathComplete)
            {
                float totalDistance = 0.0f;
                for (int i = 0; i < m_path.corners.Length - 1; i++)
                {
                    totalDistance += Vector3.Distance(m_path.corners[i], m_path.corners[i + 1]);
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

        public void RemoveTarget(Killable target)
        {
            m_targetables.RemoveAll(x => x.target == target);
            onKilled?.Invoke(target);
        }

        public void AddTarget(Killable target)
        {
            if (target.team == m_gameObject.GetComponent<Killable>().team)
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

            onSpawned?.Invoke(target);
        }
    }
}