using System;
using R3;

namespace ZE.MechBattle
{
    public abstract class Player : IDisposable
    {
        public ITargetDesignator TargetDesignator { get; private set; }
        public MechController MechController { get; private set; }
        public readonly CompositeDisposable LifetimeObject = new();
    
        public virtual void SetDesignator(ITargetDesignator targetDesignator) => TargetDesignator = targetDesignator;
        public virtual void SetMech(MechController mech)
        {
            MechController = mech;
            MechController.AssignPlayer(this);
        }

        public virtual void Dispose()
        {
            LifetimeObject.Dispose();
        }
    }
}
