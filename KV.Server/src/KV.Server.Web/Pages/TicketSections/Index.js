$(function ()
{
    var l = abp.localization.getResource('Server');

    var crudTicketSectionsService = kV.server.tickets.crudTicketSection;
    var createModal = new abp.ModalManager(abp.appPath + 'TicketSections/CreateModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'TicketSections/EditModal');

    var dataTable = $('#TicketSectionsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(crudTicketSectionsService.getList),
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
                                visible: abp.auth.isGranted('TicketSection.Edit'),
                                action: function (data)
                                {
                                    console.log(data.record);
                                    updateModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('TicketSection.Delete'),
                                action: function (data)
                                {
                                    crudTicketSectionsService.delete(data.record.id)
                                        .then(function ()
                                        {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data)
                                {
                                    return l('Global:Notify:DeleteConfirm', data.record.name);
                                }
                            }
                        ]
                    }
                },
                {
                    title: l("TicketSection:Name"),
                    data: "name"
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

    $('#CreateNewTicketSection').click(function (e)
    {
        e.preventDefault();
        createModal.open();
    });
});