$(function ()
{
    var l = abp.localization.getResource('Server');
    var storiesAppService = kV.server.storiesManagement;
    var createModal = new abp.ModalManager(abp.appPath + 'Stories/CreateSliderModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'Stories/EditSliderModal');
    var storiesId = document.getElementById("StoryId").value;
    var dataTable = $('#Sliders').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(storiesAppService.getSlidersList, {
                storyId: storiesId
            }),
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
                                    storiesAppService.deleteSlide(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data) {
                                    return l('Global:Notify:DeleteConfirm', data.record.imageUrl);
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('Sliders:CoverUrl'),
                    data: "imageUrl"
                },
                {
                    title: l('Sliders:OrderIndex'),
                    data: "orderIndex"
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
    $('#CreateNewSliderButton').click(function (e) {
        e.preventDefault();
        createModal.open({ storyId: storiesId });
    });
});