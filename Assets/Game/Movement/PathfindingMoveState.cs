using UnityEngine;
using ZE.MechBattle.Ecs;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs.States 
{ 
    public class PathfindingMoveState : DefaultMoveState
    {
        protected readonly NavigationMapController NavMap;

        public PathfindingMoveState(World world, NavigationMapController navMap, TransformAspectHandler transformAspectHandler) : base(world, transformAspectHandler)
        {
            NavMap = navMap;
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var point = TransformAspectHandler.GetPoint(entity);
            var targetPos = MoveTargets.Get(entity).WorldPos;

           //var hex = TriangularMath.WorldToHex(point.pos,)
           return StateKey.Move;
        }
    }
}
