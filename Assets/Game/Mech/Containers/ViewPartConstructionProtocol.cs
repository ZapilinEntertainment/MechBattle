using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ViewPartConstructionProtocol
    {
        public ViewPartConstructionMode ConstructionMode;
        public ViewPartKey ViewPartKey;
    }

    public enum ViewPartConstructionMode : byte { SyncWithViewPart, EntityOnly, SpecialMode }
}
