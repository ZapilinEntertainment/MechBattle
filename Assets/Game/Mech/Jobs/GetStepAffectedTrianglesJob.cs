using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct FootAffectionData
    {
        public readonly float2 Size;
        public readonly int CheckHexRadius;

        public FootAffectionData(float2 size, int checkHexRadius)
        {
            Size = size;
            CheckHexRadius = checkHexRadius;
        }
    }

    [BurstCompile]
    public struct GetStepAffectedTrianglesJob : IJobParallelFor
    {
        public float TriangleHeight;
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<StepTargetPointComponent> StepTargets;
        [ReadOnly] public NativeList<FootAffectionData> FootAffectionData;
        [WriteOnly] public NativeList<MechStepOccupationData>.ParallelWriter StepAffectedCells;

        public readonly struct TrisAddOperator : AffectedTrianglesCalculator.IAffectedCellsOperator
        {
            private readonly NativeList<MechStepOccupationData>.ParallelWriter _writer;
            private readonly Entity _footEntity;

            public TrisAddOperator(NativeList<MechStepOccupationData>.ParallelWriter writer, Entity footEntity)
            {
                _writer = writer;
                _footEntity = footEntity;
            }

            public void OnCellAffected(IntTriangularPos tripos)
            {
                _writer.AddNoResize(new(tripos, _footEntity)); 
            }
        }

        public void Execute(int index)
        {
            var footEntity = Filter[index];
            var stepTargetPoint = StepTargets.Get(footEntity).Value;
            var footAffectionData = FootAffectionData[index];

            var addOperator = new TrisAddOperator(StepAffectedCells, footEntity);
            AffectedTrianglesCalculator.CalculateRectAffectedTriangles(
                addOperator,
                stepTargetPoint,
                footAffectionData.Size,
                TriangleHeight,
                footAffectionData.CheckHexRadius);
        }
    }
}
