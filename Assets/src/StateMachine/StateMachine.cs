using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD57
{
    public class StateMachine<State> where State : System.Enum
    {
        public State currentState = default;
        
        private Dictionary<State, InternalState> m_states = new();
        private State lastConfiguredState;

        public void ChangeState(State state)
        {
            var previous = m_states[currentState];
            if (!currentState.Equals(state) && m_states[currentState].permittedTransitions.Contains(state))
            {
                previous.stateHandler.OnExit?.Invoke(state);
            }

            Debug.Log($"Transitioning from {currentState} to {state}");

            currentState = state;
            m_states[currentState].stateHandler.OnEnter?.Invoke(previous.state);
        }

        public void Update()
        {
            if (m_states.ContainsKey(currentState))
            {
                m_states[currentState].stateHandler.OnUpdate?.Invoke();
            }
        }

        public StateMachine<State> Configure(State state, StateHandler<State> stateHandler = null)
        {
            m_states.Add(state, new InternalState
            {
                state = state,
                stateHandler = stateHandler ?? new(),
                permittedTransitions = new()
            });

            lastConfiguredState = state;

            return this;
        }

        public StateMachine<State> Permit(State allowedTransitionState)
        {
            if (!m_states.ContainsKey(lastConfiguredState))
            {
                return null;
            }
            
            m_states[lastConfiguredState].permittedTransitions.Add(allowedTransitionState);
            return this;
        }

        public StateMachine<State> OnEnter(Action<State> onEnterHandler)
        {
            if (!m_states.ContainsKey(lastConfiguredState))
            {
                return null;
            }

            m_states[lastConfiguredState].stateHandler.OnEnter = onEnterHandler;
            return this;
        }

        public StateMachine<State> OnExit(Action<State> onExitHandler)
        {
            if (!m_states.ContainsKey(lastConfiguredState))
            {
                return null;
            }

            m_states[lastConfiguredState].stateHandler.OnExit = onExitHandler;
            return this;
        }

        public StateMachine<State> OnUpdate(Action onUpdateHandler)
        {
            if (!m_states.ContainsKey(lastConfiguredState))
            {
                return null;
            }

            m_states[lastConfiguredState].stateHandler.OnUpdate = onUpdateHandler;
            return this;
        }

        private class InternalState
        {
            public State state;
            public StateHandler<State> stateHandler;
            public HashSet<State> permittedTransitions;
        }
    }
}
