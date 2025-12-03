using UnityEngine;

namespace ZE.MechBattle
{
    // note: IView can be no-mono object (drawn through Graphics.DrawMesh)
    public interface IMonoView : IView
    {
        public Transform Transform { get;}    
    }
}
