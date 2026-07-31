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
        

        public void Execute(int index)
        {
            var footEntity = Filter[index];
            var stepTargetPoint = StepTargets.Get(footEntity).Value;

            var stepTripos = TriangularMath.WorldToTrianglePos(stepTargetPoint.pos, TriangleHeight);
            var virtualHexCenter = GetClosestVertexTriposCommand.Execute(stepTargetPoint.pos, TriangleHeight, stepTripos);
            var footAffectionData = FootAffectionData[index];

            var invertedRotation = math.inverse(stepTargetPoint.rot);
            var affectedCells = 0;
            foreach (var tripos in new HexTrianglesEnumerator(virtualHexCenter, footAffectionData.CheckHexRadius))
            {
                var worldPos = TriangularMath.TriangularToWorld(tripos, TriangleHeight);
                var inversedTranslation = worldPos - stepTargetPoint.pos;
                var localPos = math.mul(invertedRotation, inversedTranslation);
                
                if (math.abs(localPos.x) < footAffectionData.Size.x && math.abs(localPos.z) < footAffectionData.Size.y)
                {
                    StepAffectedCells.AddNoResize(new(tripos, footEntity));
                    affectedCells++;
                }                
            }
           // UnityEngine.Debug.Log($"affected cells: {affectedCells} / center: {virtualHexCenter} / stepPos: {stepTargetPoint.pos} / {stepTripos}");
            //if (affectedCells == 0)
            //{
            //    foreach (var tripos in new HexTrianglesEnumerator(virtualHexCenter, footAffectionData.CheckHexRadius))
            //    {
            //        var worldPos = TriangularMath.TriangularToWorld(tripos, TriangleHeight);
            //        var inversedTranslation = worldPos - stepTargetPoint.pos;
            //        var localPos = math.mul(invertedRotation, inversedTranslation);

            //        UnityEngine.Debug.Log($"{tripos} : {localPos}");
            //    }
                    
            //}
        }
    }
}
