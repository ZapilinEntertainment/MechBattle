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
    [BurstCompile]
    public struct GetStepAffectedTrianglesJob : IJobParallelFor
    {
        public float TriangleHeight;
        [ReadOnly] public NativeFilter Filter;
        [ReadOnly] public NativeStash<StepProgressionComponent> StepProgressions;
        [ReadOnly] public NativeStash<StepTargetPointComponent> StepTargets;
        [ReadOnly] public NativeStash<ChassisSettingsComponent> ChassisSettingsComponents;
        [WriteOnly] public NativeList<MechStepOccupationData>.ParallelWriter StepAffectedCells;
        

        public void Execute(int index)
        {
            var chassisEntity = Filter[index];
            var leftLegTurn = StepProgressions.Get(chassisEntity).LeftLegTurn;
            var footSize = ChassisSettingsComponents.Get(chassisEntity).FootSize;
            var stepTargetPoint = StepTargets.Get(chassisEntity).Value;

            var radiusSq = (footSize.x * footSize.x + footSize.y * footSize.y) * 0.25f;
            var stepTripos = TriangularMath.WorldToTrianglePos(stepTargetPoint.pos, TriangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(stepTargetPoint.pos, TriangleHeight, stepTripos);
            var radius = (int)math.round(math.sqrt(radiusSq) / TriangleHeight) + 1;

            var invertedRotation = math.inverse(stepTargetPoint.rot);
            foreach (var tripos in new HexTrianglesEnumerator(virtualHexCenter, radius))
            {
                var worldPos = TriangularMath.TriangularToWorld(tripos, TriangleHeight);
                var inversedTranslation = worldPos - stepTargetPoint.pos;
                var localPos = math.mul(invertedRotation, inversedTranslation);
                if (math.abs(localPos.x) < footSize.x && math.abs(localPos.y) < footSize.y)
                    StepAffectedCells.AddNoResize(new(tripos, chassisEntity));
            }
        }
    }
}
