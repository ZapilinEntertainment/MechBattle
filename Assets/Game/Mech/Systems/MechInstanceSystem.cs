using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechInstanceSystem : EntityCreationSystemBase<MechInstanceRequestComponent, MechFactory>
    {
        private Stash<PlayerAffiliationComponent> _affiliations;

        public MechInstanceSystem(MechFactory factory) : base(factory)
        {
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
        }

        protected override bool TryExecuteRequest(Entity requestEntity)
        {
            var request = RequestsStash.Get(requestEntity);
            var entity = Factory.Build(request.Position, request.Rotation);
            _affiliations.Set(entity, new(request.PlayerKey));
            return true;
        }
    }
}