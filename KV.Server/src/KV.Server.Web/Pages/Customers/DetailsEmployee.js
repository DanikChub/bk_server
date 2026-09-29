$(function () 
{
    var l = abp.localization.getResource('Server');
    var customerId = $('#Customer_Id').attr('data-customer-id');
    var createModal = new abp.ModalManager(abp.appPath + 'Customers/CreateEmployeeModal');

    var customerUserProfileService = kV.server.employees.customerUserProfiles;

    let urlParams = new URLSearchParams(window.location.search);
    let firstName = urlParams.get('Search.FirstName');
    let lastName = urlParams.get('Search.LastName');
    let middleName = urlParams.get('Search.MiddleName');
    let dateOfBirthDay = urlParams.get('Search.DateOfBirthDay');
    let jobPost = urlParams.get('Search.JobPost');
    document.getElementById('Search_FirstName').value = firstName;
    document.getElementById('Search_LastName').value = lastName;
    document.getElementById('Search_MiddleName').value = middleName;
    document.getElementById('Search_DateOfBirthDay').value = dateOfBirthDay;
    document.getElementById('Search_JobPost').value = jobPost;

    var getListByCustomerId = function(input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.appPath + 'api/app/customer-user-profiles' + abp.utils.buildQueryString([
                { name: 'searchFirstName', value: firstName },
                { name: 'searchLastName', value: lastName },
                { name: 'searchMiddleName', value: middleName },
                { name: 'searchDateOfBirthDay', value: dateOfBirthDay },
                { name: 'searchJobPost', value: jobPost },
                { name: 'customerId', value: customerId },
                { name: 'sorting', value: input.sorting }, { name: 'skipCount', value: input.skipCount }, { name: 'maxResultCount', value: input.maxResultCount }]) + '',
            type: 'GET'
        }, ajaxParams));
    };

    var dataTable = $('#EmployeesTable').DataTable(
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
                                text: l('Details'),
                                action: function (data)
                                {
                                    $(location).prop('href', '/Customers/DetailsEmployee/' + data.record.id);
                                }
                            },
                            {
                                text: l('Delete'),
                                action: function (data)
                                {
                                    customerUserProfileService.delete(data.record.id)
                                        .then(function ()
                                        {
                                            abp.notify.success(l('Global:Notify:Success'));
                                            dataTable.ajax.reload();
                                        });
                                },
                                confirmMessage: function (data)
                                {
                                    return l('Global:Notify:DeleteConfirm', data.record.lastName);
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
    createModal.onResult(function ()
    {
        dataTable.ajax.reload();
    });
    $('#reset').click(function (e) {
        firstName = null;
        lastName = null;
        dateOfBirthDay = null;
        middleName = null;
        jobPost = null;
        setTimeout(() => $('#sub').click(), 500)
        dataTable.ajax.reload();
    });
    $('#CreateNewEmployee').click(function (e)
    {
        let dataCustomerId = $('#Customer_Id').attr('data-customer-id');
        createModal.open({ Id: dataCustomerId });
    });
});