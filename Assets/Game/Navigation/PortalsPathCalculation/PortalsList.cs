using System.Collections.Generic;

namespace ZE.MechBattle.Navigation.PortalPathCalculation
{
    internal interface IPortalsList
    {
        void Clear();
        void Add(PortalOption option);
    }

    internal class StartPortalsList : List<PortalOption>, IPortalsList { }
    internal class EndPortalsList : Dictionary<int, PortalOption>, IPortalsList
    {
        public void Add(PortalOption option) => Add(option.PortalId, option);
    }
}
