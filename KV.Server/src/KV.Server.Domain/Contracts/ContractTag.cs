namespace KV.Server.Contracts;
using System;
using KV.Server.Tags;
using Volo.Abp.Domain.Entities;

public class ContractTag : Entity
{
    /// <summary>
    ///     Ссылка на нужный контракт
    /// </summary>
    public Guid ContractId { get; set; }

    public Contract Contract { get; set; }

    /// <summary>
    ///     Ссылка на нужный тег
    /// </summary>
    public Guid TagId { get; set; }

    public Tag Tag { get; set; }

    public override object[] GetKeys() => new object[] { this.ContractId, this.TagId };
}
