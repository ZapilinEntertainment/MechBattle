using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexTransitionableNodes : IReadOnlyCollection<HexPathNodeKey>
    {
        private readonly HashSet<HexPathNodeKey> _nodes;

        public HexTransitionableNodes(HashSet<HexPathNodeKey> nodes)
        {
            _nodes = nodes;
        }

        #region IReadOnlyCollection
        public int Count => _nodes.Count;
        public IEnumerator<HexPathNodeKey> GetEnumerator() => _nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
        #endregion

        public bool IsNodeTransitionable(int2 hexCoord, int edgeIndex) => IsNodeTransitionable(hexCoord, (HexEdge)edgeIndex);
        public bool IsNodeTransitionable(int2 hexCoord, HexEdge edge) => IsNodeTransitionable(new HexPathNodeKey(hexCoord, edge));

        public bool IsNodeTransitionable(HexPathNodeKey node) => _nodes.Contains(node) || _nodes.Contains(node.ToOpposite());

      
    }
}
