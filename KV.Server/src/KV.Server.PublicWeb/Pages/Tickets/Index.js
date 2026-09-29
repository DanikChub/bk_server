$(function () {
    const renderTooltip = (data) => {
        const str = `${data}`;
        const placement = 'data-placement="top"';
        return data
            ? `<div data-toggle="tooltip" ${placement} title='${str}'>${data}</div>`
            : `<div data-toggle="tooltip" ${placement} title='Отсутствует'>Отсутствует</div>`;
    };


    const renderDateWithTooltip = (data) => {
        const str = `${moment(data).format("DD.MM.YYYY")}`
        return `<div data-toggle="tooltip" data-placement="top" title='${str}'>${str}</div>`
    }


    const ticketStatus = {
        'требуется уточнение': '#ED5565',
        'в работе': '#1AB394',
        'закрыта': '#A7B1C2 ',
        'новая': '#F8AC59',
        'ожидание ответа': '#F86657',
        'черновик': '#99CCFF'
    };

    const ticketsComplect = {
        сайт: '#FFD887',
        консалтинг: '#78B7D9',
        it: '#7AE68D',
        absent: '#BC89FF',
    };

    var l = abp.localization.getResource('Server');
    let urlParams = new URLSearchParams(window.location.search);
    let ticketStatusId = urlParams.get('TicketStatusId');
    if (ticketStatusId == null) {
        ticketStatusId = $("#TicketStatusId").val();
    }
    let subject = urlParams.get('Subject');

    const ticketsList = (input, ajaxParams) => {
        return abp.ajax(
            $.extend(
                true,
                {
                    url:
                        abp.appPath +
                        'api/app/tickets-public/tickets-list' +
                        abp.utils.buildQueryString([
                            { name: 'sorting', value: input.sorting },
                            { name: 'skipCount', value: input.skipCount },
                            { name: 'ticketStatusId', value: ticketStatusId },
                            { name: 'searchSubject', value: subject },
                            {
                                name: 'maxResultCount',
                                value: input.maxResultCount,
                            },
                        ]) +
                        '',
                    type: 'GET',
                },
                ajaxParams
            )
        );
    };

    const dataTable = $('#TicketsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, 'asc']],
            searching: false,
            selected: true,
            scrollX: true,
            info: false,
            lengthChange: false,
            stripeClasses: ['stripe_tickets '],
            ajax: abp.libs.datatables.createAjax(ticketsList),
            columnDefs: [
                {
                    title: 'Статус заявки',
                    data: 'ticketStatusDisplayName',
                    width: '20%',
                    render: function (data) {
                        return data
                            ? `<p  data-toggle="tooltip" title="${data}" class="tickets-status-item" style="background: ${ticketStatus[data.toLowerCase()]
                            }">${data}</p>`
                            : ``;
                    },
                },
                {
                    title: 'Тема',
                    data: 'subject',
                    width: '37.5%',
                    render: (data, _, row) => ` <a href='/tickets/details/${row.id}'>
                                             ${data}
                                            </a>`
                },
                {
                    title: 'Номер заявки',
                    data: 'id',
                    width: '10%',
                    render: (data, _, row) => ` <a href='/tickets/details/${row.id}'>
                                             ${data}
                                            </a>`
                },
                {
                    title: 'Ответственный',
                    data: 'responsibleFullName',
                    width: '20%',
                    render: (data, _, row) => {
                        const displayData = data ? `<span title='${data}'> ${data}</span>` : 'Отсутствует';
                        return displayData;
                    }
                },
                {
                    title: 'Дата создания',
                    data: 'creationTime',
                    width: '25%',
                    render: renderDateWithTooltip,
                },
                {
                    width: '2.5%',
                    data: 'id',
                    render: (data) => ` <a href='/tickets/details/${data}'>
                                                <img src='assets/table/edit.svg' alt='redirect'></img>
                                            </a>`,
                },
            ],
        })
    );
});
