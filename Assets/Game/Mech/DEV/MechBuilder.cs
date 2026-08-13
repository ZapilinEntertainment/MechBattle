using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Obsolete]
    public class MechBuilder
    {
        public MechController Build()
        {
            var mech = new MechController();
            mech.Init();
            var mechView = GameObject.FindFirstObjectByType<TEST_MechView>();

            mech.RightWeapon = mechView.TEST_RightGun;
            mech.LeftWeapon = mechView.TEST_LeftGun;

            return mech;
        }
    }
}
