using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    // chat-gpt generated

    public enum NavigationNodeState : byte
    {
        None = 0,
        Open = 1,
        Closed = 2
    }

    public struct NavigationNode
    {
        public int Cost;
        public int Heuristics;
        public int Parent;
        public NavigationNodeState State;
        public int EdgesPassabilityMask;

        public bool IsEdgePassable(int edge) => (EdgesPassabilityMask & (1 << edge)) != 0;
    }

    [BurstCompile]
    public struct ConstructHexPathJob : IJob
    {
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public NativeArray<int2> NeighborOffsets;

        public int2 Start;
        public int2 Target;

        public NativeArray<NavigationNode> Nodes;
        public NativeList<int> Heap;
        public NativeList<int2> Result;
       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int ToIndex(int2 c)
        {
            return c.x + c.y * Width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int2 ToCoord(int index)
        {
            return new int2(index % Width, index / Width);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Inside(int2 c)
        {
            return c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height;
        }

        int HexDistance(int2 a, int2 b)
        {
            int x1 = a.x;
            int z1 = a.y;
            int y1 = -x1 - z1;

            int x2 = b.x;
            int z2 = b.y;
            int y2 = -x2 - z2;

            return math.max(
                math.abs(x1 - x2),
                math.max(
                    math.abs(y1 - y2),
                    math.abs(z1 - z2)
                )
            );
        }

        void HeapPush(int node)
        {
            Heap.Add(node);
        }

        int HeapPop()
        {
            int top = Heap[0];
            Heap.RemoveAtSwapBack(0);
            return top;
        }

        void BuildPath(int index)
        {
            while (index != -1)
            {
                int2 c = ToCoord(index);
                Result.Add(c);

                index = Nodes[index].Parent;
            }

            // reverse
            for (int i = 0; i < Result.Length / 2; i++)
            {
                int j = Result.Length - 1 - i;

                int2 tmp = Result[i];
                Result[i] = Result[j];
                Result[j] = tmp;
            }
        }

        public void Execute()
        {
            Result.Clear();
            Heap.Clear();

            int startIndex = ToIndex(Start);
            int targetIndex = ToIndex(Target);

            NavigationNode start = Nodes[startIndex];
            start.Cost = 0;
            start.Heuristics = HexDistance(Start, Target);
            start.Parent = -1;
            start.State = NavigationNodeState.Open;

            Nodes[startIndex] = start;

            HeapPush(startIndex);

            int closestIndex = startIndex;
            int closestDist = start.Heuristics;

            while (Heap.Length > 0)
            {
                int currentIndex = HeapPop();
                NavigationNode current = Nodes[currentIndex];

                current.State = NavigationNodeState.Closed;
                Nodes[currentIndex] = current;

                int2 coord = ToCoord(currentIndex);

                if (currentIndex == targetIndex)
                {
                    BuildPath(currentIndex);
                    return;
                }

                int dist = HexDistance(coord, Target);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = currentIndex;
                }

                for (int i = 0; i < 6; i++)
                {
                    if (!Nodes[ToIndex(coord)].IsEdgePassable(i))
                        continue;

                    int2 n = coord + NeighborOffsets[i];

                    if (!Inside(n))
                        continue;

                    int ni = ToIndex(n);

                    NavigationNode neighbor = Nodes[ni];

                    if (neighbor.State == NavigationNodeState.Closed)
                        continue;

                    int g = current.Cost + 1;

                    if (neighbor.State != NavigationNodeState.Open || g < neighbor.Cost)
                    {
                        neighbor.Cost = g;
                        neighbor.Heuristics = HexDistance(n, Target);
                        neighbor.Parent = currentIndex;

                        Nodes[ni] = neighbor;

                        if (neighbor.State != NavigationNodeState.Open)
                        {
                            neighbor.State = NavigationNodeState.Open;
                            Nodes[ni] = neighbor;
                            HeapPush(ni);
                        }
                    }
                }
            }

            BuildPath(closestIndex);
        }
    }

    }
