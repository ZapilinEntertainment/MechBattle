using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexTransitionableNodes : IReadOnlyCollection<HexEdgeKey>
    {
        private readonly HashSet<HexEdgeKey> _nodes;

        public HexTransitionableNodes(HashSet<HexEdgeKey> nodes)
        {
            _nodes = nodes;
        }

        #region IReadOnlyCollection
        public int Count => _nodes.Count;
        public IEnumerator<HexEdgeKey> GetEnumerator() => _nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
        #endregion

        public bool IsNodeTransitionable(int2 hexCoord, int edgeIndex) => IsNodeTransitionable(hexCoord, (HexEdge)edgeIndex);
        public bool IsNodeTransitionable(int2 hexCoord, HexEdge edge) => IsNodeTransitionable(new HexEdgeKey(hexCoord, edge));

        public bool IsNodeTransitionable(HexEdgeKey node) => _nodes.Contains(node) || _nodes.Contains(node.ToOpposite());

      
    }
}
