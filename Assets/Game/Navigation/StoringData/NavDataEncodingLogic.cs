using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.DataStoring
{
    public static class NavDataEncodingLogic
    {
        [BurstCompile]
        public static void Encode(NativeSlice<byte> writeSlice, IntTriangularPos pos, FlowMapCombinedCell cellData)
        {
            ByteCoder.WriteInt3ToBufferLittleEndian(writeSlice, pos.ToInt3(), 0);
            FlowMapCombinedCell.Encode(writeSlice, cellData, IntTriangularPos.SERIALIZATION_LENGTH);
        }

        [BurstCompile]
        public static (IntTriangularPos pos, FlowMapCombinedCell cellData) Decode(NativeSlice<byte> readSlice)
        {
            var pos = new IntTriangularPos(ByteCoder.ReadInt3FromBufferLittleEndian(readSlice, 0));
            var cellData = FlowMapCombinedCell.Decode(readSlice, IntTriangularPos.SERIALIZATION_LENGTH);
            return (pos, cellData);
        }
    }
}
