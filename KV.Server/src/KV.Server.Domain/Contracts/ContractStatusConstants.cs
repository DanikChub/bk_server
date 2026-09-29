using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Contracts;
/// <summary>
/// Класс хранит статусы договора
/// </summary>
public class ContractStatusConstants
{
    public const string STATUS_CODE_PROGRESS = "in progress";
    public const string STATUS_CODE_CANCEL = "cancel";
    public const string STATUS_CODE_COMPLETE = "complete";
    public const string STATUS_CODE_DRAFT = "draft";

}
