$(function () {
    var l = abp.localization.getResource('Server');
    var contractId = document.getElementById("EditContract_Id").value;
    var createModal = new abp.ModalManager(abp.appPath + 'Customers/CreateServicePackageModal');
    var createTagModal = new abp.ModalManager(abp.appPath + 'Customers/CreateTagModal');

    $('#add-package').click(function (e) {
        e.preventDefault();
        let contractId = $(this).attr("contract-id");
        createModal.open({ id: contractId });
    });
    $('#add-tag').click(function (e) {
        e.preventDefault();
        let contractId = $(this).attr("contract-id");
        createTagModal.open({ id: contractId });
    });
    $('.deleteTag').click(async function (e) {
        e.preventDefault();
        let tagId = $(this).attr("tag-id");
        let confirmed = await abp.message.confirm('Вы действительно хотите удалить тег');

        if (confirmed) {
            await kV.server.crudContract.deleteTagById(tagId);
            location.reload();
        }

    });

   
    createTagModal.onResult(function () {
        location.reload();
    });
    createModal.onResult(function () {
        window.open('/Customers/EditContract/' + contractId, "_self");
    });

    $('.tags__el').each(function () {
        $(this).find('.tags__delete').click(async function (e) {
            e.preventDefault();

            let packageId = $(this).attr("service-package-id"); // Получаем service-package-id из текущего элемента
            let confirmed = await abp.message.confirm('Вы действительно хотите удалить пакет услуг');

            if (confirmed) {
                await kV.server.crudServicePackage.deleteServicePackageById(contractId, packageId);
                window.open('/Customers/EditContract/' + contractId, "_self");
            }
        });
    })

});