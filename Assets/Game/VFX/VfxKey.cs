namespace ZE.MechBattle
{
    public struct VfxKey
    {
        public bool IsDefined => _isDefined;
        public int IdKey;   
        private bool _isDefined;

        public VfxKey(int idKey)
        {
            IdKey = idKey;
            _isDefined = true;
        }

        public static VfxKey Empty = new() { _isDefined = false};
    }
}
