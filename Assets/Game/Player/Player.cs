using System;
using R3;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    public abstract class Player : IDisposable
    {
        public ITargetDesignator TargetDesignator { get; private set; }
        public MechController MechController { get; private set; }
        public Entity EcsEntity { get; private set; }
        public readonly CompositeDisposable LifetimeObject = new();
    
        public Player(World world)
        {
            EcsEntity = world.CreateEntity();
        }

        public virtual void SetDesignator(ITargetDesignator targetDesignator) => TargetDesignator = targetDesignator;
        public virtual void SetMech(MechController mech)
        {
            MechController = mech;
            MechController.SetPlayerAffinity(this);
        }

        public virtual void Dispose()
        {
            LifetimeObject.Dispose();
        }
    }
}
