using System.Collections.Generic;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle 
{
    public class UpdateEdgeExitsRequestsList : HashSet<BothSideHexEdge> 
    {
        public void Add(int2 hexCoord, HexEdge edge) => Add(new BothSideHexEdge(new HexEdgeKey(hexCoord,edge)));

        public void Add(int2 hexCoord)
        {
            for (var i = 0; i < 6; i++) 
            {
                Add(hexCoord, (HexEdge)i);
            }
        }
    }
}