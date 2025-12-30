using System.Collections.Generic;   
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{

    public enum PeakNeighbour : byte
    {
        VertexUp,
        VertexUpRight,
        EdgeUpRight,
        VertexRight,
        VertexDownRightValley,
        VertexDownRightPeak,
        EdgeDown,
        VertexDownLeftPeak,
        VertexDownLeftValley,
        VertexLeft,
        EdgeUpLeft,
        VertexUpLeft
    }

    public enum ValleyNeighbour : byte
    {
        EdgeUp,
        VertexUpRightValley,
        VertexUpRightPeak,
        VertexRight, 
        EdgeDownRight,
        VertexDownRight,
        VertexDown,
        VertexDownLeft,
        EdgeDownLeft,
        VertexLeft,
        VertexUpLeftPeak,
        VertexUpLeftValley
    }
}
