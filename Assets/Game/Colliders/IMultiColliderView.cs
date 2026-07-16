using System.Collections.Generic;

namespace ZE.MechBattle
{
    public interface IMultiColliderView
    {
        void FillCollidersList(ICollection<int> colliderInstanceIds);
    
    }
}
