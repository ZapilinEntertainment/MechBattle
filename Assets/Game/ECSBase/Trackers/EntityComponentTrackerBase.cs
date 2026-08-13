using Scellecs.Morpeh;
using System.Threading.Tasks;
using UnityEngine;

namespace ZE.MechBattle
{
    public abstract class EntityComponentTrackerBase<T> where T : struct, IComponent
    {
        protected enum TrackingStatus : byte { ContinueWait, ConditionMatched, StopTracking};

        protected readonly Stash<T> Stash;
        private readonly World _world;

        protected Entity Entity;              
    
        public EntityComponentTrackerBase(World world)
        {
            _world = world;
            Stash = _world.GetStash<T>();
        }

        public void StartTracking(Entity entity)
        {
            Entity = entity;
            var conditionResult = CheckCondition();
            if (conditionResult == TrackingStatus.ContinueWait)
                WaitUntilConditionMatched();
        }

        abstract protected bool IsStashConditionMatch();
        abstract protected void OnConditionMatched();

        private TrackingStatus CheckCondition()
        {
            if (_world?.IsDisposed(Entity) ?? true)
                return TrackingStatus.StopTracking;

            return IsStashConditionMatch() ? TrackingStatus.ConditionMatched : TrackingStatus.ContinueWait;
        }   
        
        private async Awaitable WaitUntilConditionMatched()
        {
            TrackingStatus result;
            do
            {
                result = CheckCondition();
                await Awaitable.NextFrameAsync();
            }
            while (result == TrackingStatus.ContinueWait);

            if (result == TrackingStatus.ConditionMatched)
                OnConditionMatched();
        }
    }
}
