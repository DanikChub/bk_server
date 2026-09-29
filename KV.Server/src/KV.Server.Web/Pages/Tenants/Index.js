$(function ()
{
    var l = abp.localization.getResource('Server');

    var crudTenantProfileAppService = kV.server.cRUDTenantProfile;
    var createModal = new abp.ModalManager(abp.appPath + 'Tenants/CreateModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'Tenants/EditModal');

    var dataTable = $('#TenantProfiles').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(crudTenantProfileAppService.getList),
            columnDefs:
            [
                {
                    title: l('Actions'),
                    rowAction:
                    {
                        items:
                        [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('TenantProfileManagement.Edit'),
                                action: function (data)
                                {
                                    console.log(data.record);
                                    updateModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('TenantProfileManagement.Delete'),
                                action: function (data)
                                {
                                    crudTenantProfileAppService.delete(data.record.id)
                                        .then(function ()
                                        {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data)
                                {
                                    return l('Global:Notify:DeleteConfirm', data.record.shortName);
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('TenantProfile:ShortName'),
                    data: "shortName"
                },
                {
                    title: l('TenantProfile:LongName'),
                    data: "longName"
                },
                {
                    title: l('TenantProfile:Address'),
                    data: "address"
                }
            ]
        })
    );

    createModal.onResult(function ()
    {
        dataTable.ajax.reload();
    });

    updateModal.onResult(function ()
    {
        dataTable.ajax.reload();
    });

    $('#CreateNewTenantProfileButton').click(function (e)
    {
        console.log(e);
        e.preventDefault();
        createModal.open();
    });
});