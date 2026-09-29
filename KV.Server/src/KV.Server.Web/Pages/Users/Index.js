$(async function () {
    $('#my-users-button').click(async function (e) {
        e.preventDefault();
        var resId = $(this).data('responsible-id');
        window.open('/Users?UserSearchViewModelData.ResponsibleManagerId=' + `${resId}`, '_self');
    });

    const actualizationTime = createColumnField(
        'Актуализация',
        'User_ActualizationTime',
        'User.ActualizationTime'
    );

    const filterParams = [
        { name: 'ФИО', field: 'FullName', type: "text" },
        { name: 'Должность', field: 'JobPost', type: "text" },
        { name: 'Направление деятельности', field: 'Direction', type: "text" },
        { name: 'Наименование клиента', field: 'TenantShortName', type: "text" },
        { name: 'Актуализация', field: 'ActualizationTime', type: "date" },
    ];

    const columnsData = [
        {
            title: setTableTitle('Статус', 68),
            width: "2.5%",
            data: 'isActive',
            render: (_, __, data) => {
                let status;
                let tooltipValue;
                if (data.isActive) {
                    status = '/img/status/status-green-ok.svg';
                    tooltipValue = "Активный"
                }
                else {
                    status = '/img/status/status-gray-minus.svg';
                    tooltipValue = "Заблокированный"
                }
                return `<div class="table-wrapper"><img src="${status}" alt="status" data-toggle="tooltip" title='${tooltipValue}'/></div>`;
            },
            orderable: false,
        },
        {
            title: setTableTitle('ФИО', 240),
            data: 'fullName',
            width: '20%',
            render: (data, _, row) => `<a href="/Customers/DetailsEmployee/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data.trim() == "" ? "Отсутствует" : data}</a>`
        },
        {
            title: setTableTitle('Должность', 550),
            data: 'jobPost',
            width: '52.5%',
            render: renderTooltip,
        },
        {
            title: setTableTitle('Направление деятельности', 100),
            data: 'direction',
            width: '12.5%',
            render: renderTooltip,
        },
        {
            title: setTableTitle('Наименование клиента', 100),
            data: 'tenantShortName',
            width: '12.5%',
            render: renderTooltip,
        },
        {
            title: setTableTitle('Актуализация', 100),
            data: "actualizationTime",
            width: '6%',
            render: renderDateWithTooltip
        }
    ];

    const SearchParams = [
        { name: 'searchFullName', value: 'User.FullName' },
        { name: 'searchJobPost', value: 'User.JobPost' },
        { name: 'searchDirection', value: 'User.Direction' },
        { name: 'searchTenantShortName', value: 'User.TenantShortName' },
        { name: 'searchActualizationTime', value: 'User.ActualizationTime' },
        { name: 'searchResponsibleManagerId', value: 'UserSearchViewModelData.ResponsibleManagerId' }
    ];

    const tableIdModalApiPath = {
        modalPath: {},
        apiPath: "api/app/users",
        id: "#UsersTable"
    };

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
        filterPrefix: 'User'
    });
});