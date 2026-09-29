using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace KV.Server.BusinessExceptions;
public class ExistingUserException : BusinessException
{
    public ExistingUserException() : base(ServerDomainErrorCodes.UserAlreadyExists)
    {
    }

    public ExistingUserException(string message)
        : base(ServerDomainErrorCodes.UserAlreadyExists)
    {
    }
}
