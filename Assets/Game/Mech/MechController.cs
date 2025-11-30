using System;
using UnityEngine;
using R3;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle
{
    // todo: create player / mech dictionary, change to mech controller factory binding
    public class MechController : IDisposable
    {
        public readonly CompositeDisposable LifetimeObject = new();
        public MechWeapon RightWeapon;
        public MechWeapon LeftWeapon;

        public void Init()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => Fire())
                .AddTo(LifetimeObject);
        }

        public void SetPlayerAffinity(Player player)
        {
            RightWeapon.SetPlayerAffinity(player.EcsEntity);
            RightWeapon.SetDesignator(player.TargetDesignator);

            LeftWeapon.SetPlayerAffinity(player.EcsEntity);
            LeftWeapon.SetDesignator(player.TargetDesignator);
        }

        public void Fire()
        {
            RightWeapon.Fire();
            LeftWeapon.Fire();
        }

        public void Dispose()
        {
            LifetimeObject.Dispose();
        }       

        public MechWeapon[] GetWeapons() => new MechWeapon[2] { LeftWeapon, RightWeapon };
    }
}
