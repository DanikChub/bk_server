$(function ()
{
    var l = abp.localization.getResource('Server');

    var crudAnswerTemplatesService = kV.server.crudAnswerTemplate;
    var createModal = new abp.ModalManager(abp.appPath + 'AnswerTemplates/CreateModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'AnswerTemplates/EditModal');

    var dataTable = $('#AnswerTemplatesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(crudAnswerTemplatesService.getList),
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
                                visible: abp.auth.isGranted('AnswerTemplate.Edit'),
                                action: function (data)
                                {
                                    console.log(data.record);
                                    updateModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('AnswerTemplate.Delete'),
                                action: function (data)
                                {
                                    crudAnswerTemplatesService.delete(data.record.id)
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
                    title: l("AnswerTemplate:Title"),
                    data: "title"
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

    $('#CreateNewAnswerTemplate').click(function (e)
    {
        e.preventDefault();
        createModal.open();
    });
});