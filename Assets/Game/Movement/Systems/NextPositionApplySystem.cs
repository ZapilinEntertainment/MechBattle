using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NextPositionApplySystem : PausableSystem
    {
        private Filter _filter;
        private Stash<NextPositionComponent> _nextPositions;
        private readonly TransformAspectHandler _handler;
        private readonly ChildEntitiesUpdateHandler _childEntitiesUpdateHandler;


        [Inject]
        public NextPositionApplySystem(SceneFlagsManager flags, TransformAspectHandler handler, ChildEntitiesUpdateHandler childEntitiesUpdateHandler) : base(flags)
        {
            _handler = handler;
            _childEntitiesUpdateHandler = childEntitiesUpdateHandler;
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<NextPositionComponent>().Build();
            _nextPositions = World.GetStash<NextPositionComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused) 
                return;

            foreach (var entity in _filter)
            {
                var nextPos = _nextPositions.Get(entity).WorldPos;
                _handler.SetPosition(entity, nextPos);
            }
            _nextPositions.RemoveAll();

            _childEntitiesUpdateHandler.UpdateChildPositions();
        }
    }
}