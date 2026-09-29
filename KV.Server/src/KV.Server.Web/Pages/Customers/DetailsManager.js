$(function () {
    var l = abp.localization.getResource('Server');
    var customerId = $('#Customer_Id').attr('data-customer-id');
    var createModal = new abp.ModalManager(abp.appPath + 'Customers/CreateManagerModal');

    var customerUserProfileService = kV.server.employees.customerUserManager;

    var getListByCustomerId = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.appPath + 'api/app/customer-user-manager/by-tenant-id' + abp.utils.buildQueryString([
                { name: 'customerId', value: customerId },
                { name: 'sorting', value: input.sorting }, { name: 'skipCount', value: input.skipCount }, { name: 'maxResultCount', value: input.maxResultCount }]) + '',
            type: 'GET'
        }, ajaxParams));
    };

    var dataTable = $('#ManagersTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(getListByCustomerId),
            columnDefs:
                [
                    {
                        title: l('Actions'),
                        rowAction:
                        {
                            items:
                                [
                                    {
                                        text: l('Delete'),
                                        action: function (data) {
                                            customerUserProfileService.deleteManagerByTenantId(data.record.id, customerId)
                                                .then(function () {
                                                    abp.notify.success(l('Global:Notify:Success'));
                                                    dataTable.ajax.reload();
                                                });
                                        },
                                        confirmMessage: function (data) {
                                            return l('Global:Notify:DeleteConfirmManager', "(" + data.record.lastName + " " + data.record.firstName + ")");
                                        }
                                    }
                                ]
                        }
                    },
                    {
                        title: l("Employee:LastName"),
                        data: "lastName"
                    },
                    {
                        title: l("Employee:FirstName"),
                        data: "firstName"
                    },
                    {
                        title: l("Employee:MiddleName"),
                        data: "middleName"
                    },
                    {
                        title: l("Employee:JobPost"),
                        data: "jobPost"
                    },
                    {
                        title: l("Employee:DateOfBirthDay"),
                        data: "dateOfBirthDay",
                        dataFormat: "date"
                    }
                ]
        })
    );
    createModal.onResult(function () {
        dataTable.ajax.reload();
    });
    $('#CreateNewManager').click(function (e) {
        let dataCustomerId = $('#Customer_Id').attr('data-customer-id');
        console.log(e);
        createModal.open({ Id: dataCustomerId });
    });
});