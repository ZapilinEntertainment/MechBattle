using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public static class ByteCoder
    {

        [BurstCompile]
        public static void WriteIntToBufferLittleEndian(NativeSlice<byte> buffer, int value, int offset)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        [BurstCompile]
        public static int ReadIntFromBufferLittleEndian(NativeSlice<byte> buffer, int offset) => 
            buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);

        [BurstCompile]
        public static void WriteInt3ToBufferLittleEndian(NativeSlice<byte> buffer, int3 value, int offset)
        {
            WriteIntToBufferLittleEndian(buffer, value.x, offset);
            WriteIntToBufferLittleEndian(buffer, value.y, offset + 4);
            WriteIntToBufferLittleEndian(buffer, value.z, offset + 8);
        }

        [BurstCompile]
        public static int3 ReadInt3FromBufferLittleEndian(NativeSlice<byte> buffer, int offset)
        {
            var x = ReadIntFromBufferLittleEndian(buffer, offset);
            var y = ReadIntFromBufferLittleEndian(buffer, offset + 4);
            var z = ReadIntFromBufferLittleEndian(buffer, offset + 8);
            return new(x,y,z);
        }

        [BurstCompile]
        public static void WriteShortToBufferLittleEndian(NativeSlice<byte> buffer, short value, int offset)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }


        [BurstCompile]
        public static void WriteShortToBufferLittleEndian(IList<byte> buffer, short value, int offset)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        [BurstCompile]
        public static short ReadShortFromBufferLittleEndian(NativeSlice<byte> buffer, int offset) =>
           (short)(buffer[offset] | (buffer[offset + 1] << 8));

        [BurstCompile]
        public static short ReadShortFromBufferLittleEndian(IReadOnlyList<byte> byteArray, int offset) =>
           (short)(byteArray[offset] | (byteArray[offset + 1] << 8));


        
    }
}
