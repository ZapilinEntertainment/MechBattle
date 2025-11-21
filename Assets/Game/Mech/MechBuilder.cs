using UnityEngine;

namespace ZE.MechBattle
{
    public class MechBuilder
    {
        public MechController Build()
        {
            var mech = new MechController();
            var mechView = GameObject.FindFirstObjectByType<MechView>();

            mech.RightWeapon = mechView.TEST_RightGun;
            mech.LeftWeapon = mechView.TEST_LeftGun;

            return mech;
        }
    }
}
