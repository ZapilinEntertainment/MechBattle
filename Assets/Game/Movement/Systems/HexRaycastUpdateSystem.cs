using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexRaycastUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private readonly HexRaycastRequestsList _requestsList;

        [Inject]
        public HexRaycastUpdateSystem(HexRaycastRequestsList requestsList)
        {
            _requestsList = requestsList;
        }

        public void OnAwake() 
        {

        }

        public void OnUpdate(float deltaTime) 
        {

        }

        public void Dispose()
        {

        }
    }
}