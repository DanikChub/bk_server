$(async function () {
    const contractFinishDate = createColumnField("Дата окончания", "Contract_ContractFinishDate", "Contract.ContractFinishDate");

    const filterParams = [
        {
            name: 'Статус', field: 'StatusId', type: "select", getItems: async () => {
                const res = await window.kV.server.crudContractStatus.getContractStatusesList();
                return res.map((status) => ({
                    text: status.title,
                    value: status.id,
                }));
            }
        },
        { name: 'Название клиента', field: 'TenantProfileShortName', type: "text" },
        { name: 'Номер договора', field: 'Name', type: "text" },
        {
            name: 'Пакет Услуг', field: 'ServicePackage', type: "select", getItems: async () => {
                const res =
                    await window.kV.server.crudServicePackage.getListServicePacakage();

                return res.map((servicePackage) => ({
                    text: servicePackage.name,
                    value: servicePackage.id,
                }));
            }
        },
        { name: 'Дата начала', field: 'ContractStartDate', type: "date" },
        { name: 'Дата окончания', field: 'ContractFinishDate', type: "date" },
    ];

    const columnsData = [
        {
            width: "2.5%",
            title: setTableTitle('Статус', 68),
            data: "contractFinishDate",
            render: function (data) {
                let finishContract = luxon.DateTime.fromISO(data, {
                    locale: abp.localization.currentCulture.name
                });
                let checkedHtml = Date.now() > finishContract ? "" : "checked='checked'";
                return `<div class="table-wrapper">
                            <input class='checkbox' disabled type='checkbox' " + ${checkedHtml} + " />
                            <label for='checkbox1' class='checkbox__label'/>
                        </div>
                `;
            },
            orderable: false,
        },
        {
            title: setTableTitle('Название клиента', 495),
            data: "tenantProfileShortName",
            width: "35%",
            render: (data, _, row) => `<a href="/Customers/EditContract/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`
        },
        {
            title: setTableTitle('Номер договора', 282),
            data: "name",
            render: (data) => renderTooltip(data)
        },
        {
            title: setTableTitle('Пакет услуг', 147),
            data: "servicePackagesData",
            width: "10.5%",
            render: (data) => {
                if (data) {
                    var dataObj = JSON.parse(data);
                    return dataObj.map((obj) => {
                        return `<span data-toggle="tooltip" title="${obj.Name}" class="package-srvices" style="background: ${obj.Style.replace('color:', ' ')}; display: block; text-align: center;">${obj.Name}</span>`;
                    });
                } else {
                    return "Отсутствует";
                }
            },
        },
        {
            title: setTableTitle('Дата начала', 164),
            data: "contractStartDate",
            render: (data) => renderDateWithTooltip(data)
        },
        {
            title: setTableTitle('Дата окончания', 164),
            data: "contractFinishDate",
            render: (data) => renderDateWithTooltip(data)
        }
    ]

    const SearchParams = [
        { name: 'searchStatusId', value: 'Contract.StatusId' },
        { name: 'searchTenantProfileShortName', value: 'Contract.TenantProfileShortName' },
        { name: 'searchName', value: 'Contract.Name' },
        { name: 'searchServicePackageId', value: 'Contract.ServicePackageId' },
        { name: 'searchContractStartDate', value: 'Contract.ContractStartDate' },
        { name: 'searchContractFinishDate', value: 'Contract.ContractFinishDate' }
    ]

    const tableIdModalApiPath = {
        modalPath: { create: "Contracts/CreateContractModal", update: "Contracts/EditContractModal" },
        apiPath: "api/app/crud-contract",
        id: "#ContractsTable"
    }

    const abpConfiguration = {
        serverSide: true,
        paging: true,
        order: [[1, 'asc']],
        searching: false,
        scrollX: false,
        stripeClasses: [],
        info: false,
        autoWidth: true,
        lengthPage: true,
        lengthMenu: [10, 25, 50, 75, 100],

        fnDrawCallback: function (oSettings) {
            if (oSettings._iDisplayLength > oSettings.fnRecordsDisplay()) {
                $(oSettings.nTableWrapper).find('.dataTables_paginate').hide();
            } else {
                $(oSettings.nTableWrapper).find('.dataTables_paginate').show();
            }
        }
    }

    new Table({
        columnsData,
        URLSearchParams: SearchParams,
        tableIdModalApiPath,
        defaultViewForFilterFields: initCompleteFunc,
        abpConfiguration,
        filterParams,
        filterPrefix: 'Contract'
    });

    var createModal = new abp.ModalManager({ viewUrl: abp.appPath + 'Contracts/CreateContractModal', modalClass: 'CreateContract' });
    $('#createContractButton').click(function (e) {
        createModal.open();
    });

    createModal.onResult(function (formdata, responseData) {
        window.open("/Customers/EditContract/" + responseData.responseText.id);
    });
})