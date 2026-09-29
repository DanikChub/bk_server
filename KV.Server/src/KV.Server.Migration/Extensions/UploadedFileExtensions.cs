namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.File;

public static class UploadedFileExtensions
{
    public static void SetCreationTime(this UploadedFile file, DateTime CreationTime)
    {
        var info = typeof(UploadedFile).GetProperty("CreationTime");
        info?.SetValue(file, CreationTime);
    }

    public static void SetCreatorId(this UploadedFile file, Guid? CreatorId)
    {
        var info = typeof(UploadedFile).GetProperty("CreatorId");
        info?.SetValue(file, CreatorId);
    }
}
