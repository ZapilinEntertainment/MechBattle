using ZE.Utils;

namespace ZE.MechBattle.Views
{
    public class VirtualViewContainersFactory
    {
        private class VirtualContainersPool : PoolBase<VirtualViewContainer>
        {
            protected override VirtualViewContainer Instantiate() => new();
        }

        private readonly VirtualContainersPool _containersPool;

        public VirtualViewContainersFactory()
        {
            _containersPool = new();
        }

        public VirtualViewContainer Get() => _containersPool.Get();

    }
}
