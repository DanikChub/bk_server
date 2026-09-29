using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Permissions.Tickets;
public class TicketAttachmentsPermissions
{
    public const string GroupName = "TicketAttachments";

    public static class TicketAttachments
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
