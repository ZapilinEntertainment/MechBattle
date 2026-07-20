using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct CellVertexWeights
    {
        public float PinnacleWeight => _value.x;
        public float LeftBasisWeight => _value.y;
        public float RightBasisWeight => _value.z;
        private readonly float3 _value;

    
        public CellVertexWeights(float pinnacleWeight, float leftBasisWeight, float rightBasisWeight)
        {
            _value = new float3(pinnacleWeight, leftBasisWeight, rightBasisWeight);
        }

        public float this[TriangleVertex vertex]
        {
            get
            {
                switch(vertex)
                {
                    case TriangleVertex.LeftBasis: return LeftBasisWeight;
                    case TriangleVertex.RightBasis: return RightBasisWeight;
                    default: return PinnacleWeight;
                }
            }
        }
    }
}
