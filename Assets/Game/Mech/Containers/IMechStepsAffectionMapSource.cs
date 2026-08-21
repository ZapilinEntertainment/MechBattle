using System;
using ZE.MechBattle.Navigation;
using Scellecs.Morpeh;

namespace ZE.MechBattle.MechMovement
{
    public interface IMechStepsAffectionMapSource
    {
        bool IsAffectionMapEmpty { get; }
        void GetStepAffectedCells(Action<IntTriangularPos, Entity> addAffectionData);
        void ClearData();
    }
}
