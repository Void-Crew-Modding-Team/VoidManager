#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace VoidManager.MPModChecks
{
    public class MPUserDataBlock
    {
        public MPUserDataBlock(string VoidManagerVersion, MPModDataBlock[] ModData)
        {
            this.VMVersion = VoidManagerVersion;
            this.ModData = ModData;
        }

        public MPUserDataBlock()
        {
            this.VMVersion = string.Empty;
            this.ModData = null;
        }

        public string VMVersion { get; }
        public MPModDataBlock[] ModData { get; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
