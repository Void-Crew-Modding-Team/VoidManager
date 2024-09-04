namespace VoidManager.MPModChecks
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public enum PlayersWithMod : byte
    {
        Client,
        Host,
        Both
    }

    public enum CheckFailReason : byte
    {
        NoFail,
        MismatchedVersions,
        AllClientLacking,
        AllHostLacking,
        Session,
        Custom
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
