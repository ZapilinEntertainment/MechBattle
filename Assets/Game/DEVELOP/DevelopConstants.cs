namespace ZE.MechBattle
{
    public static class DevelopConstants
    {
        public const string DEFAULT_MECH_ID = "default_mech";
        public const string DEFAULT_MECH_GUN_ID = "default_mech_gun";
        public const string DEFAULT_RAY_EFFECT_ID = "default_ray_effect";
        public const string LASER_EYES_WEAPON_ID = "laser_eyes_weapon";

        public const float TEMP_MainGunDamage = 10f;
        public const float TEMP_EyesDamage = 100f;

#if UNITY_EDITOR
        // scripting define symbols 
        private const string NAVIGATION_LOGGER_SDS = "ZE_NAVIGATION_DEBUG"; // use for control navigation logic using NavigationLogger
        private const string JOBS_TRACKING_SDS = "MORPEH_JOB_TRACKING"; // will print message on World.JobHandle scheduling
#endif
    }
}
