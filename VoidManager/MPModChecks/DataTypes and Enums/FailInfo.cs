using static VoidManager.MPModChecks.MPModCheckManager;

namespace VoidManager.MPModChecks
{
    /// <summary>
    /// The response from CheckMod
    /// </summary>
    public struct FailInfo
    {
        /// <summary>
        /// The response from CheckMod
        /// </summary>
        public FailInfo()
        {
        }

        /// <summary>
        /// An error message for custom fail reasons.
        /// </summary>
        public string FailMessage = string.Empty;

        /// <summary>
        /// The error reason.
        /// </summary>
        public CheckFailReason CheckFailReason;

        //Tracks failling mod with fail reason.
        internal MPModDataBlock FailingMod;
    }
}
