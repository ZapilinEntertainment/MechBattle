namespace ZE.MechBattle.Navigation
{
    public readonly struct HexSectorRowsEnumerationSetup
    {
        // note: replace enum with int3 will result in execution time raising - because of size and prediction
        // however, depends on processor architecture

        public readonly PeakNeighbour PeakColumnOffset;
        public readonly PeakNeighbour PeakRowOffset;
        public readonly ValleyNeighbour ValleyColumnOffset;
        public readonly ValleyNeighbour ValleyRowOffset;
        public readonly bool IsStartTrianglePeak;
        public readonly IntTriangularPos Pinnacle;

        public HexSectorRowsEnumerationSetup(HexSector sector, IntTriangularPos pinnacleTripos)
        {
            Pinnacle = pinnacleTripos;
            switch (sector)
            {
                case HexSector.TopRight:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeDown;
                        PeakRowOffset = PeakNeighbour.VertexUpRight;

                        ValleyColumnOffset = ValleyNeighbour.EdgeDownRight;
                        ValleyRowOffset = ValleyNeighbour.VertexUpRightValley;

                        IsStartTrianglePeak = true;
                        break;
                    }
                case HexSector.BottomRight:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeDown;
                        PeakRowOffset = PeakNeighbour.VertexRight;

                        ValleyColumnOffset = ValleyNeighbour.EdgeDownLeft;
                        ValleyRowOffset = ValleyNeighbour.VertexRight;

                        IsStartTrianglePeak = false;
                        break;
                    }
                case HexSector.Bottom:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeUpLeft;
                        PeakRowOffset = PeakNeighbour.VertexDownRightPeak;

                        ValleyColumnOffset = ValleyNeighbour.EdgeDownLeft;
                        ValleyRowOffset = ValleyNeighbour.VertexDownRight;

                        IsStartTrianglePeak = true;
                        break;
                    }
                case HexSector.BottomLeft:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeUpLeft;
                        PeakRowOffset = PeakNeighbour.VertexDownLeftPeak;

                        ValleyColumnOffset = ValleyNeighbour.EdgeUp;
                        ValleyRowOffset = ValleyNeighbour.VertexDownLeft;

                        IsStartTrianglePeak = false;
                        break;
                    }
                case HexSector.TopLeft:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeUpRight;
                        PeakRowOffset = PeakNeighbour.VertexLeft;

                        ValleyColumnOffset = ValleyNeighbour.EdgeUp;
                        ValleyRowOffset = ValleyNeighbour.VertexLeft;

                        IsStartTrianglePeak = true;
                        break;
                    }
                default:
                    {
                        PeakColumnOffset = PeakNeighbour.EdgeUpRight;
                        PeakRowOffset = PeakNeighbour.VertexUpLeft;

                        ValleyColumnOffset = ValleyNeighbour.EdgeDownRight;
                        ValleyRowOffset = ValleyNeighbour.VertexUpLeftValley;

                        IsStartTrianglePeak = false;
                        break;
                    }
            }
        }
    }
}
