using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigationMapBuilder
    {
        public static NavigatonMap Build(float2 bottomLeftCorner, float2 topRightCorner, in MapSettings mapSettings)
        {
            var width = topRightCorner.x - bottomLeftCorner.x;
            var length = topRightCorner.y - bottomLeftCorner.y;
            var center = new float2(bottomLeftCorner.x + width * 0.5f, bottomLeftCorner.y + length * 0.5f);           

            var map = new NavigatonMap(new(center.x, 0f, center.y), mapSettings);        

            return map;
        }    
    }
}
