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

        public static ViewPartKey Head => new(ViewPartType.Head, 0);
        public static ViewPartKey UpperPart => new(ViewPartType.UpperPart, 0);
        public static ViewPartKey Chassis => new(ViewPartType.ChassisRoot, 0);
        public static ViewPartKey GetHipKey(bool isRight) => new(ViewPartType.Hip, SideToIndex(isRight));
        public static ViewPartKey GetAnkleKey(bool isRight) => new(ViewPartType.Ankle, SideToIndex(isRight));
        public static ViewPartKey GetFootKey(bool isRight) => new(ViewPartType.Foot, SideToIndex(isRight));

        public const int LEFT_INDEX = 0;
        public const int RIGHT_INDEX = 1;

        private static int SideToIndex(bool isRight) => isRight ? RIGHT_INDEX : LEFT_INDEX;

        public override string ToString() => $"{Type} : {Index}";
    }
}
