using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public struct FlowMapCombinedCell
    {
        public byte TopEdgeDirection;
        public byte TopRightEdgeDirection;
        public byte BottomRightEdgeDirection;
        public byte BottomEdgeDirection;
        public byte BottomLeftEdgeDirection;
        public byte TopLeftEdgeDirection;

        public byte this[HexEdge edge]
        {
            get
            {
                switch(edge)
                {
                    case HexEdge.UpRight: return TopRightEdgeDirection;
                    case HexEdge.DownRight: return BottomRightEdgeDirection;
                    case HexEdge.Down: return BottomEdgeDirection;
                    case HexEdge.DownLeft: return BottomLeftEdgeDirection;
                    case HexEdge.UpLeft: return TopLeftEdgeDirection;
                    default: return TopEdgeDirection;
                }
            }
        }
    }
}
