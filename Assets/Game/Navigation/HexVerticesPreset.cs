using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public readonly struct HexVerticesPreset
    {
        public readonly float2 TopRight;
        public readonly float2 Right;
        public readonly float2 BottomRight;
        public readonly float2 BottomLeft;
        public readonly float2 Left;
        public readonly float2 TopLeft;

        public float2 this[int index] => index switch
        {
            5 => Left,
            4 => BottomLeft,
            3 => BottomRight,
            2 => Right,
            1 => TopRight,
            _ => TopLeft
        };

        public HexVerticesPreset(float hexEdgeLength)
        {
            var axis = math.up();
            var fwd = TriangularMath.DirY;
            TopRight = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(30f)), fwd).xz;
            Right = new(hexEdgeLength, 0f);
            BottomRight = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(150f)), fwd).xz;

            axis = math.down();
            BottomLeft = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(150f)), fwd).xz;
            Left = new(-hexEdgeLength, 0f);
            TopLeft = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(30f)), fwd).xz;
        }

        public SectorVerticesEnumerator GetSectorVerticesEnumerator() => new(this);
    
        public struct SectorVerticesEnumerator
        {
            private readonly HexVerticesPreset _preset;
            private int _index;

            public SectorVerticesEnumerator(HexVerticesPreset preset)
            {
                _preset = preset;
                _index = -1;
                Current = default;
            }

            public (float2, float2) Current { get; private set; }

            public bool MoveNext()
            {
                _index++;
                Current = (HexSector)_index switch
                {
                    HexSector.TopLeft => (_preset.Left, _preset.TopLeft), 
                    HexSector.BottomLeft => (_preset.BottomLeft, _preset.Left),
                    HexSector.Bottom => (_preset.BottomRight, _preset.BottomLeft),
                    HexSector.BottomRight => (_preset.Right, _preset.BottomRight),
                    HexSector.TopRight => (_preset.TopRight, _preset.Right),
                    _ => (_preset.TopLeft, _preset.TopRight)
                };

                return _index < 6;
            }

            public SectorVerticesEnumerator GetEnumerator() => this;
        }
    }
}
