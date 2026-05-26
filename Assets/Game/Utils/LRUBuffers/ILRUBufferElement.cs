namespace ZE.Utils
{
    public interface ILRUBufferElement
    {
        float LastUseTime { get; }
        void UpdateUseTime();
    }
}
