$(function ()
{
    var l = abp.localization.getResource('Server');
    var storiesAppService = kV.server.storiesManagement;
    var createModal = new abp.ModalManager(abp.appPath + 'Stories/CreateStoryModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'Stories/EditStoryModal');
    var getSlidesModal = new abp.ModalManager(abp.appPath + 'Stories/GetSlidesModal');
    var dataTable = $('#Stories').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(storiesAppService.getStoriesList),
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
                                visible: abp.auth.isGranted('StoriesManagement.Edit'),
                                action: function (data) {
                                    updateModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('StoriesManagement.Delete'),
                                action: function (data) {
                                    storiesAppService.deleteStory(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data) {
                                    return l('Global:Notify:DeleteConfirm', data.record.groupName);
                                }
                            },
                            {
                                text: l('Stories:Slides'),
                                action: function (data) {
                                    window.open("/Stories/Sliders/" + data.record.id);
                                }
                            }
                        ]
                    },
                    width: "150px"
                },
                {
                    title: l('Stories:GroupName'),
                    data: "groupName"
                },
                {
                    title: l('Stories:Name'),
                    data: "name"
                },
                {
                    title: l('Дата создания'),
                    data: "creationTime",
                    dataFormat: "date"
                },
                {
                    title: l('Stories:Status'),
                    data: "status",
                    render: function (val) {
                        return val == 1 ? "active" : "archive";
                    }
                }
            ]
        })
    );
    updateModal.onResult(function () {
        dataTable.ajax.reload();
    });
    createModal.onResult(function () {
        dataTable.ajax.reload();
    });
    $('#CreateNewStoryButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});