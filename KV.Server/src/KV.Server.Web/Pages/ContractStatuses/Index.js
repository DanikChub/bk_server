$(function ()
{
    var l = abp.localization.getResource('Server');

    var crudStatusesService = kV.server.crudContractStatus;
    var createModal = new abp.ModalManager(abp.appPath + 'ContractStatuses/CreateModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'ContractStatuses/EditModal');

    var dataTable = $('#ContractStatusesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(crudStatusesService.getList),
            columnDefs:
            [
                {
                    title: l("Actions"),
                    rowAction:
                    {
                        items:
                        [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('ContractStatus.Edit'),
                                action: function (data)
                                {
                                    console.log(data.record);
                                    updateModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('ContractStatus.Delete'),
                                action: function (data)
                                {
                                    crudStatusesService.delete(data.record.id)
                                        .then(function ()
                                        {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data)
                                {
                                    return l('Global:Notify:DeleteConfirm', data.record.title);
                                }
                            }
                        ]
                    }
                },
                {
                    title: l("ContractStatus:Name"),
                    data: "title"
                },
                {
                    title: l("ContractStatus:Code"),
                    data: "code"
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

    $('#CreateNewContractStatus').click(function (e)
    {
        console.log(e);
        e.preventDefault();
        createModal.open();
    });
});