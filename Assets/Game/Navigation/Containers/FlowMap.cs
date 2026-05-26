using System.Collections.Generic;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using UnityEngine;
using ZE.Utils;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class FlowMap : ILRUBufferElement
    {
        public readonly int2 HexCoord;
        public float LastUseTime { get;private set; }

        private readonly ushort[] Directions;
        private readonly FlattenedHexCoordsConverter _coordsConverter;



        public FlowMap(int2 hexCoord, in FlattenedHexCoordsConverter converter, int length)
        {
            HexCoord = hexCoord;
            _coordsConverter = converter;
            Directions = new ushort[length];
        }

       

        public int GetDirectionUnsafe(IntTriangularPos pos) => Directions[_coordsConverter.TriangularToIndex(pos)];
        public void SetDirectionUnsafe(IntTriangularPos pos, int dir) => Directions[_coordsConverter.TriangularToIndex(pos)] = (ushort)dir;

        public void UpdateUseTime() => LastUseTime = Time.time;


        public int this[int index] 
        {
            get => Directions[index];
            set => Directions[index] = (ushort)value; 
        }
    }
}
