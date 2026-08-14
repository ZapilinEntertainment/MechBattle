using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct ViewPartKey
    {
        public ViewPartType Type;
        public int Index;

        public ViewPartKey(ViewPartType type, int index)
        {
            Type = type;
            Index = index;
        }

        public ViewPartKey(ViewPartType type)
        {
            Type = type;
            Index = 0;
        }

        public bool IsValid => Type != ViewPartType.Undefined;
    }
}
