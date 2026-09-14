namespace ZE.MechBattle
{
    public readonly struct MechWeaponKey
    {
        public readonly MechWeaponGroup WeaponGroup;
        public readonly int Index;    

        public MechWeaponKey(MechWeaponGroup weaponGroup, int index)
        {
            WeaponGroup = weaponGroup;
            Index = index;
        }
    }
}
