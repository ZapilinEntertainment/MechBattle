using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public class AppBootstrap : IStartable
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public AppBootstrap(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Start()
        {
           
        }
    }
}
