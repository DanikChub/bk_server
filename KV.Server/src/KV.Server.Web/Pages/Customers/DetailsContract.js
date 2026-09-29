$(function () 
{
    var l = abp.localization.getResource('Server');
    var customerId = $('#Customer_Id').attr('data-customer-id');
    var createModal = new abp.ModalManager(abp.appPath + 'Customers/CreateModal');
    var updateModal = new abp.ModalManager(abp.appPath + 'Customers/EditModal');

    let urlParams = new URLSearchParams(window.location.search);
    let name = urlParams.get('Search.ContractName');
    let contractStartDate = urlParams.get('Search.ContractStartDate');
    let contractFinishDate = urlParams.get('Search.ContractFinishDate');
    let statusId = urlParams.get('Search.ContractStatusId');
    document.getElementById('Search_ContractName').value = name;
    document.getElementById('Search_ContractStartDate').value = contractStartDate;
    document.getElementById('Search_ContractFinishDate').value = contractFinishDate;
    document.getElementById('Search_ContractStatusId').value = statusId;

    var getListByCustomerId = function (input, ajaxParams) {
        return abp.ajax($.extend(true, {
            url: abp.appPath + 'api/app/crud-customer/contracts-by-customer-id' + abp.utils.buildQueryString([
                { name: 'tenantId', value: customerId },
                { name: 'searchName', value: name },
                { name: 'searchContractStartDate', value: contractStartDate },
                { name: 'searchContractFinishDate', value: contractFinishDate },
                { name: 'searchStatusId', value: statusId },
                { name: 'sorting', value: input.sorting }, { name: 'skipCount', value: input.skipCount }, { name: 'maxResultCount', value: input.maxResultCount }]) + '',
            type: 'GET'
        }, ajaxParams));
    };

    var dataTable = $('#ContractsCustomerTable').DataTable(
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
                                    action: function (data) {
                                        $(location).prop('href', '/Customers/EditContract/' + customerId + "?contractId=" + data.record.id);
                                    }
                                }
                            ]
                        }
                    },
                {
                    title: l("Contract:Name"),
                    data: "name"
                },
                {
                    title: l("Contract:ContractStartDate"),
                    data: "contractStartDate",
                    dataFormat: "date"
                },
                {
                    title: l("Contract:ContractFinishDate"),
                    data: "contractFinishDate",
                    dataFormat: "date"
                },
                {
                    title: l("ContractStatus:Label:Item"),
                    data: "statusTitle"
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
    $('#reset').click(function (e) {
        name = null;
        contractStartDate = null;
        contractFinishDate = null;
        statusId = null;
        setTimeout(() => $('#sub').click(), 500)
        dataTable.ajax.reload();
    });
    $('#CreateNewEmployee').click(function (e)
    {
        e.preventDefault();
        createModal.open();
    });
});