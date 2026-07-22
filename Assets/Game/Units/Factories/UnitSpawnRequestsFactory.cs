using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class UnitSpawnRequestsFactory : RequestFactoryBase<UnitSpawnRequestComponent>
    {

        [Inject]
        public UnitSpawnRequestsFactory(World world) : base(world) { }  

        public void CreateSpawnRequest(UnitKey unitKey, IntTriangularPos tripos, PlayerKey playerKey) =>
            CreateRequest(new(unitKey, new CellPoint() { Tripos = tripos }, playerKey));
    
    }
}
