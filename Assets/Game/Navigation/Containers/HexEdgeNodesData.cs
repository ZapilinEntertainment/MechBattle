using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexEdgeNodesData
    {
        public readonly int TopNodeIndex;
        public readonly int TopRightNodeIndex;
        public readonly int BottomRightNodeIndex;
        public readonly int BottomNodeIndex;
        public readonly int BottomLeftNodeIndex;
        public readonly int TopLeftNodeIndex;

        public readonly HexEdgesAccessMap AccessMap;
        public readonly int EdgesPassabilityMask;

        public const int INVALID_INDEX = -1;

        public HexEdgeNodesData(int top, int topRight, int bottomRight, int bottom, int bottomLeft, int topLeft, HexEdgesAccessMap accessMap, int edgesPassabilityMask)
        {
            TopNodeIndex = top;
            TopRightNodeIndex = topRight;
            BottomRightNodeIndex = bottomRight;
            BottomLeftNodeIndex = bottomLeft;
            BottomNodeIndex = bottomLeft;
            TopLeftNodeIndex = topLeft;

            AccessMap = accessMap;
            EdgesPassabilityMask = edgesPassabilityMask;
        }

        public int GetNodeIndex(int edgeIndex)
        {
            switch ((HexEdge)edgeIndex)
            {
                case HexEdge.Up: return TopNodeIndex;
                case HexEdge.UpRight: return TopRightNodeIndex;
                case HexEdge.DownRight: return BottomRightNodeIndex;
                case HexEdge.Down: return BottomNodeIndex;
                case HexEdge.DownLeft: return BottomLeftNodeIndex;
                case HexEdge.UpLeft: return TopLeftNodeIndex;
                default: return -1;
            }
        }

        public bool IsEdgePassable(HexEdge edge) => (EdgesPassabilityMask & (1 << (int)edge)) != 0;
    }
}
