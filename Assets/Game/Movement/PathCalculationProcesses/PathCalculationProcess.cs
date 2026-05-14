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
    public enum CalculationProcessStage : byte { Idle, Calculating, Complete }

    public abstract class PathCalculationProcess<NodeKey> : IDisposable where NodeKey : unmanaged
    {
        public CalculationProcessStage Stage => _isLaunched
               ? (_activeHandle.IsCompleted ? CalculationProcessStage.Complete : CalculationProcessStage.Calculating)
               : CalculationProcessStage.Idle;


        public int PathId { get; private set; }
        public int ProcessIteration { get; private set; }

        private JobHandle _activeHandle;
        private bool _isLaunched;

        public virtual void Launch(int pathId, NodeKey start, NodeKey end)
        {
            PathId = pathId;

            _activeHandle = LaunchJob(start, end);
            _isLaunched = true;
            ProcessIteration++;
        }

        public PathCalculationResult<NodeKey> StopAndGetResults()
        {
            _activeHandle.Complete();
            _isLaunched = false;
            ProcessIteration++;
            return FormResults();
        }

        public async void Dispose()
        {
            if (_isLaunched)
            {
                while (!_activeHandle.IsCompleted)
                    await Task.Delay(100);
            }
            _activeHandle = default;
            DisposeResources();
        }

        protected abstract PathCalculationResult<NodeKey> FormResults();
        protected abstract JobHandle LaunchJob(NodeKey start, NodeKey end);        
        protected abstract void DisposeResources();
    }
}
