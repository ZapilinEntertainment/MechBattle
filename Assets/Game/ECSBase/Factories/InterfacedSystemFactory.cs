using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class InterfacedSystemFactory<SystemType, ImplementedInterface>
        where SystemType : ISystem, ImplementedInterface
    {
        private readonly SystemType _instance;       

        [Inject]
        public InterfacedSystemFactory([Key(FeatureSystemsInstallQueue.FACTORY_KEY)] SystemType instance)
        {
            _instance = instance;
        }

        public SystemType ResolveSystem() => _instance;
        public ImplementedInterface ResolveInterface() => _instance;
    }
}
