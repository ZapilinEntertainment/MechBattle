using Scellecs.Morpeh;

namespace ZE.MechBattle.Views
{
    public interface IViewInstanceStrategy
    {
        Entity CreateViewEntity(string viewId);
    
    }
}
