

$(async function () {
    const newStatuses = await getStatusesFromServer()

    var l = abp.localization.getResource('Server');

    const actions = createColumnField(l('Actions'), "Search_TicketStatusId", "Search.TicketStatusId", [...newStatuses])

    // Уточнить имена 
    // --------------------------
    const shortName = createColumnField(l('Employee:LastName'), "Search_LastName", "Search.LastName")
    /*   const region = createColumnField("Наименование", "Search_Region", "Search.Region")*/
    const address = createColumnField(l('Employee:JobPost'), "Search_Address", "Search.Address")
    const login = createColumnField(l('Employee:Login'), "Search_ResponsibleFullName", "Search.ResponsibleFullName")
    const email = createColumnField(l('Employee:Email'), "Search_Email", "Search.Email")
    // --------------------------


    let urlParams = new URLSearchParams(window.location.search);
    $("#Search_SearchStr").val(urlParams.get('Search.SearchStr'));

    const columnsData = [
        {
            title: createColumnHeader({ main: "input" }, shortName),
            data: "lastName",
            render: (data, _, row) => `<a href="Employees/EditEmployee?id=${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`

        },
        {
            title: createColumnHeader({ main: "input" }, address),
            data: "jobPost",
            render: (data) => renderTooltip(data)
        },
        {
            title: createColumnHeader({ main: "input" }, email),
            data: "email",
            render: (data) => renderTooltip(data)
        }


    ]


    // Уточнить имена
    // --------------------------
    const SearchParams = [
        { name: 'searchTicketStatusId', value: 'Search.TicketStatusId' },
        { name: 'searchShortName', value: 'Search.ShortName' },
        { name: 'searchLongName', value: 'Search.Region' },
        { name: 'searchAddress', value: 'Search.Address' },
        { name: 'searchResponsibleFullName', value: 'Search.ResponsibleFullName' }
    ]
    // --------------------------


    const tableIdModalApiPath = {
        apiPath: "api/app/employee-profile",
        id: "#EmployeeProfiles"
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

    new Table({ columnsData, URLSearchParams: SearchParams, tableIdModalApiPath, defaultViewForFilterFields: initCompleteFunc, abpConfiguration });
});
