namespace ZE.MechBattle
{
    public interface IRestorable
    {
        bool RestoreIfSessionEnds { get;}
        void Restore();    
    }
}
