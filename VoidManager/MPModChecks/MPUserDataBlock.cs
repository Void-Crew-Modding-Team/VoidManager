namespace VoidManager.MPModChecks
{
    public class MPUserDataBlock
    {
        public MPUserDataBlock(string PMLVersion, MPModDataBlock[] ModData)
        {
            this.VMVersion = PMLVersion;
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
