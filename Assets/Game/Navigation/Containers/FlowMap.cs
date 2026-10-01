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
        public bool IsCalculated { get; private set; }  
        public float LastUseTime { get;private set; }

        private readonly ushort[] Directions;
        private readonly FlattenedHexCoordsConverter _coordsConverter;



        public FlowMap(int2 hexCoord, in FlattenedHexCoordsConverter converter, int length)
        {
            HexCoord = hexCoord;
            _coordsConverter = converter;
            Directions = new ushort[length];
        }


        public int GetDirectionUnsafe(IntTriangularPos pos)
        {
            var index = _coordsConverter.TriangularToIndex(pos);
#if UNITY_EDITOR
            var uindex = (uint)index;
            if (uindex > Directions.Length)
            {
                UnityEngine.Debug.LogError($"failed to get tripos {pos} from flow map at {HexCoord}");
                index = Directions.Length - 1;
            }                
#endif
            return Directions[index];
        }

        public void UpdateUseTime() => LastUseTime = Time.time;

        public void OnCalculated(in FlowMapCalculationResults results)
        {
            for (var i = 0; i < results.Length; i++)
            {
                Directions[i] = (ushort)results[i];
            }
            IsCalculated = true;
        }


        public int this[int index] 
        {
            get => Directions[index];
            set => Directions[index] = (ushort)value; 
        }
    }
}
