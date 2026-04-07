using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public enum HexSector : byte
    {
        Top,
        TopRight,
        BottomRight,
        Bottom,
        BottomLeft,
        TopLeft    
    }

    public static class HexSectorExtension
    {
        public static int GetDefaultFlowDirection(this HexSector sector, HexEdge edge, bool isPeak)
        {
            // if sector matches the exit edge
            var defaultEdge = (HexEdge)sector;
            if (edge == defaultEdge)
                return isPeak ? (int)defaultEdge.ToNeighbourDirectionFromPeak() : (int)defaultEdge.ToNeighbourDirectionFromValley();

            switch (sector)
            {
                case HexSector.Top:
                    {
                        // top sector
                        switch (edge)
                        {
                            case HexEdge.TopRight: return isPeak ? (int)PeakNeighbour.VertexDownRightValley : (int)ValleyNeighbour.EdgeDownRight;
                            case HexEdge.BottomRight: return isPeak ? (int)PeakNeighbour.VertexDownRightPeak : (int)ValleyNeighbour.VertexDownRight;
                            case HexEdge.Bottom: return isPeak ? (int)PeakNeighbour.EdgeDown : (int)ValleyNeighbour.VertexDown;
                            case HexEdge.BottomLeft: return isPeak ? (int)PeakNeighbour.VertexDownLeftPeak : (int)ValleyNeighbour.EdgeDownLeft;
                            case HexEdge.TopLeft: return isPeak ? (int)PeakNeighbour.VertexLeft : (int)ValleyNeighbour.EdgeDownLeft;
                        }
                        break;
                    }

                case HexSector.TopRight:
                    {
                        switch (edge)
                        {
                            case HexEdge.Top: return isPeak ? (int)PeakNeighbour.EdgeUpLeft : (int)ValleyNeighbour.VertexUpLeftPeak;
                            case HexEdge.BottomRight: return isPeak ? (int)PeakNeighbour.EdgeDown : (int)ValleyNeighbour.VertexDown;
                            case HexEdge.Bottom: return isPeak ? (int)PeakNeighbour.VertexDownLeftPeak : (int)ValleyNeighbour.VertexDownLeft;
                            case HexEdge.BottomLeft: return isPeak ? (int)PeakNeighbour.VertexDownLeftValley : (int)ValleyNeighbour.EdgeDownLeft;
                            case HexEdge.TopLeft: return isPeak ? (int)PeakNeighbour.VertexLeft : (int)ValleyNeighbour.VertexLeft;                           
                        }
                        break;
                    }
                
                case HexSector.BottomRight:
                    {
                        switch (edge)
                        {
                            case HexEdge.Top: return isPeak ? (int)PeakNeighbour.VertexUpLeft : (int)ValleyNeighbour.VertexUpLeftValley;
                            case HexEdge.TopRight: return isPeak ? (int)PeakNeighbour.VertexUp : (int)ValleyNeighbour.EdgeUp;
                            case HexEdge.Bottom: return isPeak ? (int)PeakNeighbour.VertexDownLeftValley : (int)ValleyNeighbour.EdgeDownLeft;
                            case HexEdge.BottomLeft: return isPeak ? (int)PeakNeighbour.VertexLeft : (int)ValleyNeighbour.VertexLeft;
                            case HexEdge.TopLeft: return isPeak ? (int)PeakNeighbour.EdgeUpLeft : (int)ValleyNeighbour.VertexUpLeftPeak;
                        }
                        break;
                    }

                case HexSector.Bottom:
                    {
                        switch (edge)
                        {
                            case HexEdge.Top: return isPeak ? (int)PeakNeighbour.VertexUp : (int)ValleyNeighbour.EdgeUp;
                            case HexEdge.TopRight: return isPeak ? (int)PeakNeighbour.EdgeUpRight : (int)ValleyNeighbour.VertexUpRightValley;
                            case HexEdge.BottomRight: return isPeak ? (int)PeakNeighbour.EdgeUpRight : (int)ValleyNeighbour.VertexRight;
                            case HexEdge.BottomLeft: return isPeak ? (int)PeakNeighbour.EdgeUpLeft : (int)ValleyNeighbour.VertexLeft;
                            case HexEdge.TopLeft: return isPeak ? (int)PeakNeighbour.VertexUpLeft : (int)ValleyNeighbour.VertexUpLeftValley;
                        }
                        break;
                    }

                case HexSector.BottomLeft:
                    {
                        switch (edge)
                        {
                            case HexEdge.Top: return isPeak ? (int)PeakNeighbour.VertexUpRight : (int)ValleyNeighbour.VertexUpRightValley;
                            case HexEdge.TopRight: return isPeak ? (int)PeakNeighbour.EdgeUpRight : (int)ValleyNeighbour.VertexUpRightPeak;
                            case HexEdge.BottomRight: return isPeak ? (int)PeakNeighbour.VertexRight : (int)ValleyNeighbour.VertexRight;
                            case HexEdge.Bottom: return isPeak ? (int)PeakNeighbour.VertexDownRightValley : (int)ValleyNeighbour.EdgeDownRight;
                            case HexEdge.TopLeft: return isPeak ? (int)PeakNeighbour.VertexUp : (int)ValleyNeighbour.EdgeUp;
                        }
                        break;
                    }

                case HexSector.TopLeft:
                    {
                        switch (edge)
                        {
                            case HexEdge.Top: return isPeak ? (int)PeakNeighbour.EdgeUpRight : (int)ValleyNeighbour.VertexUpRightPeak;
                            case HexEdge.TopRight: return isPeak ? (int)PeakNeighbour.EdgeUpRight : (int)ValleyNeighbour.VertexUpRightValley;
                            case HexEdge.BottomRight: return isPeak ? (int)PeakNeighbour.VertexDownRightValley : (int)ValleyNeighbour.EdgeDownRight;
                            case HexEdge.Bottom: return isPeak ? (int)PeakNeighbour.VertexDownRightPeak : (int)ValleyNeighbour.VertexDownRight;
                            case HexEdge.BottomLeft: return isPeak ? (int)PeakNeighbour.EdgeDown : (int)ValleyNeighbour.VertexDown;
                        }
                        break;
                    }
            }

           return -1;
        }
    }
}
