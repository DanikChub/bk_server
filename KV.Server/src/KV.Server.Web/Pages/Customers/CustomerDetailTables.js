$(async function () {
    const customerId = $('#Customer_Id').attr('data-customer-id');
    // const newStatuses = await getContractStatusesFromServer();
    const newPackages = await getPackagesFromServer();

    const dateFormater = (date, locale) => {
        const currentDate = new Date(date);

        const formatter = new Intl.DateTimeFormat(locale, {
            day: 'numeric',
            month: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });

        return formatter.format(currentDate);
    };

    const getStatusColor = (data) => {
        let status;
        switch (data) {
            case 'В работе':
                status = '#1AB394';
                break;
            case 'Закрыта':
                status = '#A7B1C2';
                break;
            case 'Новая':
                status = '#F8AC59';
                break;
            case 'Возобновлённые':
                status = '#ED5565';
                break;
            case 'Ожидание ответа':
                status = '#78B7D9';
                break;
            default:
                status = 'black';
                break;
        }
        return status;
    };

    const popularTags = await getPopularTagsFromServer();
    // first table
    const applicationNumber = createColumnField(
        'Номер заявки',
        'Search_TicketId',
        'Search.TicketId'
    );
    const subject = createColumnField(
        'Тема',
        'Search_Subject',
        'Search.Subject'
    );
    const tag = createColumnField(
        'Тэг',
        'Search_LastTagName',
        'Search.LastTagName',
        [...popularTags]
    );
    const created = createColumnField(
        'Создана',
        'Search_CreationTime',
        'Search.CreationTime'
    );
    const closed = createColumnField(
        'Закрыта',
        'Search_DueDate',
        'Search.DueDate'
    );

    let urlParams = new URLSearchParams(window.location.search);
    $('#Search_SearchStr').val(urlParams.get('Search.SearchStr'));

    const columns1 = [
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                        <lable>Статус</lable>
                    </div>`,
            data: 'ticketStatusDisplayName',
            render: function (data) {
                const status = getStatusColor(data);
                return `<span
                            data-toggle="tooltip"
                            data-placement="top"
                            title="${data}"
                            class="table-item-status"
                            style="background: ${status}"
                        >
                            ${data}
                        </span>`;
            },
            width: '12.5%',
        },
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                    <lable>Номер заявки</lable>
                </div>`,
            data: 'id',
            render: (data, _, row) => `<a href="/Tickets/Details/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`,
            width: '12.5%',
        },
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                    <lable>Тема</lable>
                </div>`,
            data: 'subject',
            render: (data) => renderTooltip(data),
            width: '35%',
        },
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                    <lable>Тэг</lable>
                </div>`,
            data: 'lastTagName',
            render: renderTag,
            width: '12.5%',
        },
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                    <lable>Создана</lable>
                </div>`,
            data: 'creationTime',
            render: (data) => renderDateWithTooltip(data),
            width: '12.5%',
        },
        {
            title: `<div style="display: flex;flex-direction: column;gap: 1rem;">
                    <lable>Закрыта</lable>
                </div>`,
            data: 'dueDate',
            render: (data) => renderDateWithTooltip(data),
            width: '12.5%',
        }
    ];

    const searchParams1 = [
        { name: 'searchTicketStatusId', value: 'Search.UpperTicketStatusId' },
        { name: 'searchTicketId', value: 'Search.TicketId' },
        { name: 'searchSubject', value: 'Search.Subject' },
        { name: 'searchTagId', value: 'Search.LastTagName' },
        { name: 'searchCreationTime', value: 'Search.CreationTime' },
        { name: 'searchDueDate', value: 'Search.DueDate' },
        { name: 'searchStr', value: 'Search.SearchStr' },
    ];

    const tableIdModalApiPath1 = {
        modalPath: {
            create: 'Customers/CreateCustomerModal',
            update: 'Customers/EditCustomerModal',
        },
        apiPath: 'api/app/tickets/tickets-list',
        id: '#CustomerProfiles',
    };

    const specialParams1 = [{ name: 'tenantId', value: customerId }];

    const abpConfiguration = {
        serverSide: true,
        paging: true,
        pagingType: "full_numbers",
        order: [[1, 'asc']],
        searching: false,
        scrollX: false,
        stripeClasses: [],
        info: false,
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

    const filterParams = [
        { name: 'Номер заявки', field: 'TicketId', type: "text" },
        { name: 'Тема', field: 'Subject', type: "text" },
        { name: 'Тэг', field: 'LastTagName', type: "text" },
        { name: 'Создана', field: 'CreationTime', type: "text" },
        { name: 'Закрыта', field: 'DueDate', type: "date" },
    ];

    new Table(
        {
            columnsData: columns1,
            URLSearchParams: searchParams1,
            tableIdModalApiPath: tableIdModalApiPath1,
            specialQueryParams: specialParams1,
            defaultViewForFilterFields: initCompleteFunc,
            abpConfiguration,
            filterParams,
            filterPrefix: 'Search'
        }
    );

    // second table
    const responsibleFullName = createColumnField(
        'ФИО',
        'Search_TicketResposibleFullName',
        'Search.TicketResposibleFullName'
    );
    const jobPost = createColumnField(
        'Должность',
        'Search_JobPost',
        'Search.JobPost'
    );
    const activityTheme = createColumnField(
        'Направление деятельности',
        'Search_ActivityTheme',
        'Search.ActivityTheme'
    );
    const updating = createColumnField(
        'Актуализация',
        'Search_Updating',
        'Search.Updating'
    );

    const usersColumns = [
        {
            title: 'Статус',
            data: 'isActive',
            render: (_, __, data) => {
                let status;

                if (data.isActive) {
                    status = '/img/status/status-green-ok.svg';
                }
                else {
                    status = '/img/status/status-gray-minus.svg';
                }
                return `<img src="${status}" alt="status"/>`;
            },
            width: '5%',
        },
        {
            title: 'ФИО',
            data: 'responsibleFullName',
            render: (data, _, row) => `<a href="/Customers/DetailsEmployee/${row.id}" data-toggle="tooltip" data-placement="top" 
                                        title='${row.lastName} ${row.firstName} ${row.middleName}'>
                                            ${row.lastName} ${row.firstName} ${row.middleName}
                                        </a>`,
            width: '50%',
        },
        {
            title: 'Должность',
            data: 'jobPost',
            render: (data) => renderTooltip(data),

            width: '10%',
        },
        {
            title: 'Актуализация',
            data: 'contractStartDate',

            render: (_, __, data) => {
                return renderDateWithTooltip(data.creationTime);
            },
            width: '15%',
        }
    ];

    const searchParams2 = [
        { name: 'searchTicketStatusId', value: 'Search.UpperTicketStatusId' },
        {
            name: 'searchResponsibleFullName',
            value: 'Search.ResponsibleFullName',
        },
        { name: 'searchJobPost', value: 'Search.JobPost' },
        { name: 'searchShortName', value: 'Search.ShortName' },
        { name: 'searchContractStartDate', value: 'Search.СontractStartDate' },
        { name: 'searchStr', value: 'Search.SearchStr' },
    ];

    const tableIdModalApiPath2 = {
        modalPath: {},
        apiPath: 'api/app/customer-user-profiles',
        id: '#CustomerProfilesUsers',
    };

    const abpConfiguration2 = {
        ...abpConfiguration,
        paging: false,
    }

    const specialParams2 = [{ name: 'customerId', value: customerId }];

    new Table(
        {
            columnsData: usersColumns,
            URLSearchParams: searchParams2,
            tableIdModalApiPath: tableIdModalApiPath2,
            specialQueryParams: specialParams2,
            defaultViewForFilterFields: initCompleteFunc,
            abpConfiguration: abpConfiguration2
        }
    );

    // third table

    const shortName = createColumnField(
        'Договор заключен на',
        'Contract_TenantProfileShortName',
        'Contract.TenantProfileShortName'
    );
    const fullName = createColumnField(
        'Номер договора',
        'Contract_Name',
        'Contract.Name'
    );
    const servicePackageName = createColumnField(
        'Пакет услуг',
        'Contract_ServicePackageId',
        'Contract.ServicePackageId',
        [...newPackages]
    );
    const contractStartDate = createColumnField(
        'Дата начала',
        'Contract_ContractStartDate',
        'Contract.ContractStartDate'
    );
    const contractFinishDate = createColumnField(
        'Дата окончания',
        'Contract_ContractFinishDate',
        'Contract.ContractFinishDate'
    );

    const treatiesColumns = [
        {
            title: 'Статус',
            data: 'ticketStatusDisplayName',
            render: (_, __, data) => {
                let status;

                switch (data.statusCode) {
                    case 'complete':
                        status = '/img/status-ok.png';
                        break;
                    case 'in progress':
                        status = '/img/status-lightning.png';
                        break;
                    case 'cancel':
                        status = '/img/status-cross.svg';
                        break;
                    default:
                        status = '/img/status-warning.png';
                        break;
                }
                return `<img data-toggle="tooltip" title="${data.statusTitle}" src="${status}" alt="status"/>`;
            },
            width: '10%',
        },
        {
            title: 'Номер договора',
            data: 'name',
            render: (data, _, row) => `<a href="/Customers/EditContract/${row.id}" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</a>`,
            width: '15%',
        },
        {
            title: 'Пакет услуг',
            data: 'servicePackagesData',
            render: (data) => {
                if (data) {
                    var dataObj = JSON.parse(data);
                    return dataObj.map((obj, index) => {
                        return `<span data-toggle="tooltip" title="${obj.Name}" class="package-srvices" style="background: ${obj.Style.replace('color:', ' ')}; display: block; text-align: center;">${obj.Name}</span>`;
                    });
                } else {
                    return "Отсутствует";
                }
            },
            width: '15%',
        },
        {
            title: 'Дата начала',
            data: 'contractStartDate',
            render: (data) => renderDateWithTooltip(data),
            width: '15%',
        },
        {
            title: 'Дата окончания',
            data: 'contractFinishDate',
            render: (data) => renderDateWithTooltip(data),
            width: '15%',
        },
        {
            title: 'Договор заключен на',
            data: 'contractPayer',
            render: (data) => renderTooltip(data),
            width: '30%',
        }
    ];

    const searchParams3 = [
        { name: 'searchTicketStatusId', value: 'Search.UpperTicketStatusId' },
        { name: 'searchName', value: 'Contract.Name' },
        { name: 'searchServicePackageId', value: 'Contract.ServicePackageId' },
        {
            name: 'searchContractStartDate',
            value: 'Contract.ContractStartDate',
        },
        {
            name: 'searchContractFinishDate',
            value: 'Contract.ContractFinishDate',
        },
        {
            name: 'searchContractPayer',
            value: 'Contract.ContractPayer',
        },
        { name: 'searchStr', value: 'Search.SearchStr' },
    ];

    const tableIdModalApiPath3 = {
        modalPath: {},
        apiPath: 'api/app/crud-customer/contracts-by-customer-id',
        id: '#CustomerTreaties',
    };

    const specialParams3 = [
        {
            name: 'tenantId',
            value: customerId,
        },
    ];

    new Table(
        {
            columnsData: treatiesColumns,
            URLSearchParams: searchParams3,
            tableIdModalApiPath: tableIdModalApiPath3,
            specialQueryParams: specialParams3,
            defaultViewForFilterFields: initCompleteFunc,
            abpConfiguration
        }
    );

    const optionsDate = {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
        hour: 'numeric',
        minute: 'numeric'
    };

    const eventsColumns = [
        {
            title: '',
            data: 'creationTime',
            render: (data) => {
                var date = new Date(data);
                return new Intl.DateTimeFormat('ru-RU', optionsDate).format(date);
            },
            width: '19%'
        },
        {
            title: '',
            data: 'creatorFirstName',
            render: (data, _, row) => {

                return `<div class="col-10" style="text-overflow: ellipsis; max-height: 20px;">
                           '${row.creatorLastName} ${row.creatorFirstName}'
                            ${row.action} ${row.subject} : ${row.description}
                        </div>`
            },
            width: '71%',
        },
        {
            title: '',
            data: 'url',
            render: (data) => {
                return `<a class="btn" href="${data}">Открыть</a>`;
            },
            width: '8%',
        },
    ];

    var getEventFilter = function () {
        return {
            tenantId: customerId,
            startPeriod: moment().subtract(24, 'months'),
            endPeriod: moment().add(2, 'days'),
        };
    };

    var eventsDataTable = $('#EventsLogTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            scrollX: false,
            scrollY: false,
            searching: false,
            ajax: abp.libs.datatables.createAjax(
                kV.server.crudCustomer.getCustomerEvents,
                getEventFilter
            ),
            initComplete: () => {
                $('#tableBooking_length label').replaceWith(
                    $('.form-select.form-select-sm')
                );
                $('.form-select.form-select-sm').before(
                    "<label class='pages-count-title'>Записей на странице</label>"
                );
            },
            columnDefs: eventsColumns
        })
    );
});
