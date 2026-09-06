namespace PowerTools.Core.Models
{
    public enum ModuleIconStyle
    {
        FontStyle,
        ImageStyle
    }

    public enum VersionUpdateStatus
    {
        NoUpdates,
        CheckForUpdates,
        HasNewVersion,
        Updating,
        Done
    }
}