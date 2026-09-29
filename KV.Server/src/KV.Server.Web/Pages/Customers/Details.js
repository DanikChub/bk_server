$(function () {

    const createModal = new abp.ModalManager({ viewUrl: abp.appPath + 'Customers/CreateContractModal', modalClass: "CreateContractModal" });
    const createEmployeeModal = new abp.ModalManager(abp.appPath + 'Customers/CreateEmployeeModal');
    const customerId = $('#Customer_Id').attr('data-customer-id');

    $('#createContractButton').click(function () {
        createModal.open({ id: customerId });
    });

    $('#createEmployee').click(function () {
        createEmployeeModal.open({ tenantId: customerId });
    });

    createEmployeeModal.onResult(function () {
        location.reload();
    });

    createModal.onResult(function (formdata, responseData) {
        window.open('/Customers/EditContract/' + `${responseData.responseText.id}`)
    });

    createEmployeeModal.onOpen(function () {
        $("#generate-password").on('click', generatePassword)
        $(".password__hide-password").on('click', toggleImg)
    })
});