namespace KV.Server;
using System;

public static class BlobContainers
{
    public const string DEFAULT_PUBLIC = "kv-public";
    public const string STORIES = "kv-stories-public";
    public const string ATTACHMENTS = "kv-tickets-attachments";
    public const string AVATAR_EMPLOYEE = "kv-avatar-emploees";
}

public static class StorageSettings
{
    public const string DEBUGLOCALSTORAGEDIRECTORY = "LocalUploads";
    public const string DEBUGLOCALAVATARDIRECTORY = "LocalAvatarUploads";
    public const string DEBUGLOCALTICKETSSTORAGEDIRECTORY = "LocalTicketsUploads";
    public const string DEBUGLOCALSTORIESSTORAGEDIRECTORY = "LocalStoriesUploads";
}

public static class StorageDirectories
{
    public static string GetLocalStoragePath()
    {
        var currentProjectPath = AppDomain.CurrentDomain.BaseDirectory;
        return currentProjectPath;
    }
}
