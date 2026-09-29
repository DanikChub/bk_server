namespace KV.Server.Permissions.Contracts;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class ContractPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        DefineContracts(context);
        DefineContractStatuses(context);
    }

    private static void DefineContracts(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(ContractPermissions.GroupName, L("Permission:ContractManagement"));

        var crudPermission = group.AddPermission(ContractPermissions.Contracts.Default, L("Permission:Contract"));

        crudPermission.AddChild(ContractPermissions.Contracts.Create, L("Permission:Contract.Create"));
        crudPermission.AddChild(ContractPermissions.Contracts.Edit, L("Permission:Contract.Edit"));
        crudPermission.AddChild(ContractPermissions.Contracts.Delete, L("Permission:Contract.Delete"));
        crudPermission.AddChild(ContractPermissions.Contracts.Get, L("Permission:Contract.Get"));
    }

    private static void DefineContractStatuses(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(ContractStatusesPermissions.GroupName, L("Permission:ContractStatusManagement"));

        var crudPermission =
            group.AddPermission(ContractStatusesPermissions.Statuses.Default, L("Permission:ContractStatus"));
        crudPermission.AddChild(ContractStatusesPermissions.Statuses.Create, L("Permission:ContractStatus.Create"));
        crudPermission.AddChild(ContractStatusesPermissions.Statuses.Edit, L("Permission:ContractStatus.Edit"));
        crudPermission.AddChild(ContractStatusesPermissions.Statuses.Delete, L("Permission:ContractStatus.Delete"));
        crudPermission.AddChild(ContractStatusesPermissions.Statuses.Get, L("Permission:ContractStatus.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
