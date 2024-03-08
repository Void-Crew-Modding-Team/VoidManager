namespace VoidManager.MPModChecks
{
    /// <summary>
    /// Holds data about Mods for MPModChecks
    /// </summary>
    public class MPModDataBlock
    {
        /// <summary>
        /// Creates an MPModDataBlock (With Hash)
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="ModName"></param>
        /// <param name="Version"></param>
        /// <param name="MPType"></param>
        /// <param name="DownloadID"></param>
        /// <param name="Hash"></param>
        public MPModDataBlock(string GUID, string ModName, string Version, MultiplayerType MPType, string DownloadID, byte[] Hash)
        {
            this.ModGUID = GUID;
            this.ModName = ModName;
            this.Version = Version;
            this.MPType = MPType;
            this.Hash = Hash;
            this.DownloadID = DownloadID;
        }

        /// <summary>
        /// Creates an MPModDataBlock (without Hash)
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="ModName"></param>
        /// <param name="Version"></param>
        /// <param name="MPType"></param>
        /// <param name="DownloadID"></param>
        public MPModDataBlock(string GUID, string ModName, string Version, MultiplayerType MPType, string DownloadID)
        {
            this.ModGUID = GUID;
            this.ModName = ModName;
            this.Version = Version;
            this.MPType = MPType;
            this.Hash = new byte[32];
            this.DownloadID = DownloadID;
        }


        public string ModGUID { get; }
        public string ModName { get; }
        public string Version { get; }
        public MultiplayerType MPType { get; }
        public byte[] Hash { get; }
        public string DownloadID { get; }
    }
}
