$(async function () {
    const regions = await kV.server.regions.regions.getList();

    $('#my-clients-button').click(async function (e) {
        e.preventDefault();
        var resId = $(this).data('responsible-id');
        window.open('/Customers?Search.ResponsibleManagerId=' + `${resId}`, '_self');
    });

    let urlParams = new URLSearchParams(window.location.search);
    $("#Search_SearchStr").val(urlParams.get('Search.SearchStr'));

    const filterParams = [
        { name: 'Название Клиента', field: 'LongName', type: "text" },
        { name: 'Регион', field: 'Region', type: "text" },
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
        { name: 'Ответственный', field: 'ResponsibleManager', type: "text" },
    ];

    const columnsData = [
        {
            title: setTableTitle('Статус', 100),
            width: "2.5%",
            data: 'isActive',
            render: (_, __, data) => {
                let status;

                if (data.isActive) {
                    status = '/img/status/status-green-ok.svg';
                }
                else {
                    status = '/img/status/status-gray-minus.svg';
                }
                return `<div class="table-wrapper"><img src="${status}" alt="status"/></div>`;
            },
            orderable: false,
        },
        {
            title: setTableTitle('Название Клиента', 350),
            data: "shortName",
            render: (data, _, row) => `<a href="/Customers/Details/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`

        },
        {
            title: setTableTitle('Регион', 150),
            data: "regionId",
            render: (data) => {
                if (!data) {
                    return "";
                }

                var region = regions.items.filter(obj => {
                    return obj.id === data
                });

                if (!region) {
                    return "";
                }

                var name = region[0].name;

                return name;
            }
        },
        {
            title: setTableTitle('Пакет Услуг', 150),
            data: "servicePackagesData",
            width: "10.5%",
            render: (data) => {
                if (data) {
                    var dataObj = JSON.parse(data);
                    dataObj = dataObj.filter((obj) => moment(obj.ContractFinishDate, 'YYYY-MM-DD') > moment())
                    return dataObj.map((obj) => {
                        return `<span data-toggle="tooltip" title="${obj.Name}" class="package-srvices" style="background: ${obj.Style.replace('color:', ' ')}; display: block; text-align: center; margin-bottom: 5px;">${obj.Name}</span>`;
                    }).join('');
                } else {
                    return "Отсутствует";
                }
            },
        },
        {
            // title: createColumnHeader({ main: "input" }, responsible),
            title: setTableTitle('Ответственный', 350),
            data: "responsibleManager",
            render: (data) => {
                console.log(data)
                return renderTooltip(data)
            }
        }
    ]

    // Уточнить имена
    // --------------------------
    const SearchParams = [
        { name: 'searchTicketStatusId', value: 'Search.TicketStatusId' },
        { name: 'searchShortName', value: 'Search.ShortName' },
        { name: 'searchAddress', value: 'Search.Address' },
        { name: 'searchStr', value: 'Search.SearchStr' },
        { name: 'searchResponsibleManager', value: 'Search.ResponsibleManager' },
        { name: 'searchResponsibleManagerId', value: 'Search.ResponsibleManagerId' },
        { name: 'searchLongName', value: 'Search.LongName' }
    ]
    // --------------------------


    const tableIdModalApiPath = {
        modalPath: { create: "Customers/CreateCustomerModal", update: "Customers/EditCustomerModal" },
        apiPath: "api/app/crud-customer/customers-by-search",
        id: "#CustomerProfiles"
    }

    const abpConfiguration = {
        serverSide: true,
        paging: true,
        pagingType: "full_numbers",
        order: [[1, 'asc']],
        searching: false,
        scrollX: false,
        stripeClasses: [],
        info: false,
        autoWidth: false,
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
        filterPrefix: 'Search'
    });
});
