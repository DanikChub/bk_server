$(async function () {
    let urlParams = new URLSearchParams(window.location.search);
    $('#Search_SearchStr').val(urlParams.get('Search.SearchStr'));

    const filterParams = [
        { name: 'Номер заявки', field: 'TicketId', type: "text" },
        { name: 'Наименование клиента', field: 'CustomerLongName', type: "text" },
        {
            name: 'Тэг', field: 'LastTagName', type: "select", getItems: async () => {
                const res = await window.kV.server.tags.tags.getPopularByTicketTag();
                return res.map((tag) => ({
                    text: tag.name,
                    value: tag.id,
                }));
            }
        },
        {
            name: 'Тип', field: 'TicketTypeId', type: "select", getItems: async () => {
                const res = await window.kV.server.tickets.ticketType.getTicketTypes();
                return res.map((type) => ({
                    text: type.name,
                    value: type.id,
                }));
            }
        },
        {
            name: 'Раздел', field: 'TicketSectionName', type: "select", getItems: async () => {
                const res = await window.kV.server.tickets.ticketSection.getTicketSections();
                return res.map((section) => ({
                    text: section.name,
                    value: section.id,
                }));
            }
        },
        { name: 'Специалист', field: 'ResponsibleFullName', type: "text" },
        { name: 'Дата создания', field: 'CreationTime', type: "date" },
    ];

    const columnsData = [
        {
            data: 'ticketStatusDisplayName',
            title: setTableTitle('Статус', 68),
            render: function (status) {
                if (status === 'Новая') {
                    return `<div class='table-wrapper' data-toggle="tooltip" title='${status}'><img class='status-img' src='/img/status-lightning.png'></div>`;
                }
                if (status === 'В работе') {
                    return `<div class='table-wrapper' data-toggle="tooltip" title='${status}'><img class='status-img' src='/img/status-ok.png'></div>`;
                }
                if (status === 'Возобновленные') {
                    return `<div class='table-wrapper' data-toggle="tooltip" title='${status}'><img class='status-img' src='/img/status-warning.png'></div>`;
                }
                if (status === 'Закрыта') {
                    return `<div class='table-wrapper' data-toggle="tooltip" title='${status}'><img class='status-img' src='/img/cross-gray.png'></div>`;
                }
                return `<div class='table-wrapper table-wrapper_unknown' data-toggle="tooltip" title='Неизвестен'><img class='status-img' src='/img/status/status-unknown.svg'></div>`
            },
            width: '2.5%',
            orderable: false,
        },
        {
            title: setTableTitle('Номер заявки', 111),
            data: 'id',
            render: (data, _, row) => `<a href="/Tickets/Details/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`,
            width: '10%',
        },
        {
            title: setTableTitle('Наименование клиента', 365),
            data: 'customerShortName',
            width: '35%',
            render: (data) => renderVolga(data),
        },
        {
            title: setTableTitle('Тэг', 84),
            data: 'lastTagName',
            render: (data) => renderTag(data),
            width: '10%',
        },
        {
            title: setTableTitle('Тип', 95),
            data: 'ticketTypeName',
            width: '10%',
            render: (data) => renderTooltip(data),
        },
        {
            title: setTableTitle('Раздел', 95),
            data: 'ticketSectionName',
            width: '10%',
            render: (data) => renderTooltip(data),
        },
        {
            title: setTableTitle('Специалист', 106),
            data: 'responsibleFullName',
            width: '10%',
            render: (data) => renderTooltip(data)
        },
        {
            title: setTableTitle('Дата создания', 110),
            data: 'creationTime',
            width: '10%',
            render: (data) => renderDateWithTooltip(data)
        }
    ];

    const SearchParams = [
        { name: 'searchTicketId', value: 'Search.TicketId' },
        { name: 'searchCustomerLongName', value: 'Search.CustomerLongName' },
        { name: 'searchTagId', value: 'Search.LastTagName' },
        { name: 'searchTicketStatusId', value: 'Search.UpperTicketStatusId' },
        { name: 'searchTicketTypeId', value: 'Search.TicketTypeId' },
        { name: 'searchTicketSectionName', value: 'Search.TicketSectionName' },
        { name: 'searchResponsibleFullName', value: 'Search.ResponsibleFullName' },
        { name: 'searchCreationTime', value: 'Search.CreationTime' },
        { name: 'searchStr', value: 'Search.SearchStr' },
    ];

    const tableIdModalApiPath = {
        modalPath: { update: 'Tickets/EditModal' },
        apiPath: 'api/app/tickets/tickets-list',
        id: '#Tickets',
    };

    const abpConfiguration = {
        serverSide: true,
        paging: true,
        pagingType: "full_numbers",
        order: [[1, 'asc']],
        searching: false,
        scrollX: false,
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
        },
        dom: 'Bfrtip',
        stateSave: true,
        stateSaveCallback: function (settings, data) {
            localStorage.setItem(
                'DataTables_Tickets' + settings.sInstance,
                JSON.stringify(data)
            );
        },
        stateLoadCallback: function (settings) {
            return JSON.parse(localStorage.getItem('DataTables_Tickets' + settings.sInstance));
        },
        buttons: [
            {
                extend: '',
                text: '<span>Настройка колонок</span>',
                className: 'not-btn'
            },
            {
                extend: 'colvis',
                text: '<img style="width: 20px;" src="/img/buttons/column.svg"/>',
                className: 'colvis-btn'
            }
        ]
    }

    new Table({
        columnsData,
        URLSearchParams: SearchParams,
        defaultViewForFilterFields: initCompleteFunc,
        tableIdModalApiPath,
        abpConfiguration,
        filterParams,
        filterPrefix: 'Search'
    });
});
