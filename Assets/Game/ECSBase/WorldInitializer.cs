using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class WorldInitializer : IInitializer
    {
        public World World { get;set; }       

        public void OnAwake()
        {
            World.GetStash<ViewComponent>().AsDisposable();
        }

         public void Dispose()
        {
            
        }
    }
}
