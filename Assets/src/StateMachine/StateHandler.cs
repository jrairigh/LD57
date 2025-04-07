using System;

namespace LD57
{
    public class StateHandler<State> where State : System.Enum
    {
        public Action<State> OnEnter;
        public Action OnUpdate;
        public Action<State> OnExit;
    }
}