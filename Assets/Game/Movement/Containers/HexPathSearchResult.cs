using UnityEngine;

namespace ZE.MechBattle
{
    public enum HexPathSearchResult : byte
    {
        PathImpossible, CalculationNotFinished, OnlyIncompletePathPossible, PathFound
    }

    public struct HexPathSearchResultData
    {
        public HexPathSearchResult Result;
        public int EndNode;
        public int PathId;
        public int NodesCount;
        public AwaitingToken ConstructionAwaitingToken;
    }
}
