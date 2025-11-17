namespace ZE.MechBattle
{
    // reference type- there can be some dynamic parameters (different behavior based on damage for ex.)
    public class ChassisParameters 
    {
        public readonly float HipLength;
        public readonly float AnkleLength;
        public readonly float HipsDistance;
        public float LegLength => AnkleLength + HipsDistance;

        public ChassisParameters(float hipLength, float ankleLength, float hipsDistance)
        {
            HipLength = hipLength;
            AnkleLength= ankleLength;
            HipsDistance= hipsDistance;
        }
    }
}
