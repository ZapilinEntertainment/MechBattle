using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct PathInput<NodeKey> where NodeKey : unmanaged
    {
        public readonly int PathId;
        public readonly NodeKey Start;
        public readonly NodeKey End;

        public PathInput(int pathId, NodeKey start, NodeKey end)
        {
            PathId = pathId;
            Start = start;
            End = end;
        }
    }

    public abstract class PathCalculationProcess<NodeKey> : ProcessBase<PathInput<NodeKey>, PathCalculationResult<NodeKey>> where NodeKey : unmanaged
    {
        public int PathId { get; private set; }

        public override void Launch(PathInput<NodeKey> input)
        {
            PathId = input.PathId;
            base.Launch(input);
        }
    }
}
