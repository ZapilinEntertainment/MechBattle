using System;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public readonly struct StateHandlerKey : IEquatable<StateHandlerKey>
    {
        public readonly StateKey State;
        public readonly BehaviourKey Behaviour;

        public StateHandlerKey(BehaviourKey behaviour, StateKey state)
        {
            State = state;
            Behaviour = behaviour;
        }

        // IMPORTANT: using direct equality check results in GC.Allocations, long fixes it
        public static long ToLong(StateKey stateKey, BehaviourKey behaviourKey)
=> ((long)(int)behaviourKey << 32) | (uint)(int)stateKey;

        public bool Equals(StateHandlerKey other) => State == other.State && Behaviour == other.Behaviour;

        public override bool Equals(object obj)
        {
            return obj is StateHandlerKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + State.GetHashCode();
                hash = hash * 31 + Behaviour.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(StateHandlerKey left, StateHandlerKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateHandlerKey left, StateHandlerKey right)
        {
            return !left.Equals(right);
        }
    }
}
