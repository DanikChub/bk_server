namespace KV.Server;
using System;
using KV.Server.Profiles;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Сущность компание и менджера (тот кто работает по компание)
/// </summary>
public class TenantProfileManager : Entity
{
    /// <summary>
    ///     Компания
    /// </summary>
    public Guid TenantProfileId { get; set; }

    public TenantProfile TenantProfile { get; set; }

    /// <summary>
    ///     Менджер
    /// </summary>
    public Guid ManagerId { get; set; }

    public CustomerUserProfile Manager { get; set; }

    public override object[] GetKeys() => new object[] { this.TenantProfileId, this.ManagerId };
}
