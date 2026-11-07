namespace RealismCommonLib.Integration
{
    /// <summary>
    /// Provided by Stance-Overhaul. Returns the "not installed" defaults when absent.
    /// </summary>
    public interface IStanceInfo
    {
        // left shoulder is the active stance
        bool IsLeftShoulder { get; }
    }

    internal sealed class NullStanceInfo : IStanceInfo
    {
        public bool IsLeftShoulder => false;
    }
}
