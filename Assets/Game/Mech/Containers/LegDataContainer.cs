using System;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{
    [Serializable]
    public struct LegDataContainer<T>
    {
        public T Hip;
        public T Ankle;
        public T Foot;    
    }
}
