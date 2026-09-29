namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.Profiles;

public static class CustomerUserProfileExtensions
{
    public static void SetCreationTime(this CustomerUserProfile customer, DateTime CreationTime) => customer.CreationTime = CreationTime;

    public static void SetLastModificationTime(this CustomerUserProfile customer, DateTime UpdateTime) => customer.LastModificationTime = UpdateTime;
}
