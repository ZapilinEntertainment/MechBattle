using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathClearSystem : ICleanupSystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;

        private readonly List<IStash> _clearingStashes;

        [Inject]
        public TrianglePathClearSystem(World world) 
        {
            _clearingStashes = ReceiveStashesListByComponentInterfaceCommand.Execute<ITrianglePathComponent>(world);
        }

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<ClearTrianglePathTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _clearFilter)
            {
                foreach (var stash in _clearingStashes)
                {
                    stash.Remove(entity);
                }

                //UnityEngine.Debug.Log($"triangle path data cleared for entity {entity.Id}");
            }
        }

        public void Dispose()
        {

        }
    }
}