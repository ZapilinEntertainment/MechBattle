using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.DataStoring
{
    [BurstCompile]
    public struct EncodeFlowMapDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<IntTriangularPos> Keys;
        [ReadOnly] public NativeArray<FlowMapCombinedCell> Values;

        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]public NativeArray<byte> Result;
        public const int SLICE_LENGTH = IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH;

        public void Execute(int index)
        {
            var pos = Keys[index];
            var cellData = Values[index];

            var writeSlice = Result.Slice(index * SLICE_LENGTH, SLICE_LENGTH);
            NavDataEncodingLogic.Encode(writeSlice, pos, cellData);
        }
    }

    [BurstCompile]
    public struct DecodeFlowMapDataJob : IJob
    {
        [ReadOnly] public NativeArray<byte> SourceData;
        [WriteOnly] public NativeHashMap<IntTriangularPos, FlowMapCombinedCell> ResultMap;
        public int ItemsCount;
        public const int SLICE_LENGTH = IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH;

        public void Execute()
        {
            var readIndex = 0;
            while (readIndex < SourceData.Length)
            {
                var readSlice = SourceData.Slice(readIndex, SLICE_LENGTH);
                var result = NavDataEncodingLogic.Decode(readSlice);
                ResultMap.Add(result.pos, result.cellData);
                readIndex += SLICE_LENGTH;
            }       
        }
    }
}
