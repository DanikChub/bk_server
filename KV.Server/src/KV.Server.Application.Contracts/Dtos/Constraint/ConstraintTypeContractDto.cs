namespace KV.Server;
using System;
using System.Collections.Generic;

/// <summary>
///     Отображение ограничений под контрактом (Важно! Это сущность должна создаватся каждый месяц иначе данные будут не
///     корректные)
/// </summary>
public class ConstraintTypeContractDto
{
    /// <summary>
    ///     Месяц от 1 до 12
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    ///     Контракт
    /// </summary>
    public Guid? ContractId { get; set; }

    /// <summary>
    ///     Ограничение в этом месяце (сколько часов списали по каждому ограничению)
    /// </summary>
    public List<ConstraintTypeDto> ConstraintTypes { get; set; }
}
