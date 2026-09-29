$(function () {
    var l = abp.localization.getResource('Server');
    var identityUserId = $('#EditEmployee_Id').val();
    var idForTenant = $('#EditEmployee_IdentityUserId').val();
    var updateModal = new abp.ModalManager(abp.appPath + 'Tickets/EditModal');
    var editCustomerModal = new abp.ModalManager(abp.appPath + 'Customers/EditEmployeeModal');
    var createEventModal = new abp.ModalManager(abp.appPath + 'Customers/CreateEventModal');
    var createTicketModal = new abp.ModalManager({ viewUrl: abp.appPath + 'Customers/CreateTicketModal', modalClass: 'CreateTicketModal' });
    var ticketsAppService = kV.server.tickets.tickets;
    var changePasswordModal = new abp.ModalManager(abp.appPath + 'Customers/ChangePasswordModal');
    let urlParams = new URLSearchParams(window.location.search);
    let searchCreationTime = urlParams.get('Search.TicketCreationTime');
    let searchDueDate = urlParams.get('Search.TicketDueDate');
    let searchResponsibleFullName = urlParams.get('Search.TicketResposibleFullName');
    let searchTicketStatusId = urlParams.get('Search.TicketStatusId');
    let searchCustomerShortName = urlParams.get('Search.TicketCustomerShortName');

    var dataTable = $('#TicketsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({

            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            info: false,
            fnDrawCallback: function (oSettings) {
                if (oSettings._iDisplayLength > oSettings.fnRecordsDisplay()) {
                    $(oSettings.nTableWrapper).find('.dataTables_paginate').hide();
                } else {
                    $(oSettings.nTableWrapper).find('.dataTables_paginate').show();
                }
            },
            initComplete: function () {
                const totalRecordsCount = document.getElementById(
                    `Tickets-Count`
                );

                if (totalRecordsCount) {
                    totalRecordsCount.innerText = this.api().page.info()
                        .recordsTotal
                        ? `(${this.api().page.info().recordsTotal})`
                        : '(0)';
                }
            },
            ajax: abp.libs.datatables.createAjax(kV.server.tickets.tickets.getTicketsList,
                {
                    searchTicketStatusId,
                    searchCreationTime,
                    searchDueDate,
                    searchResponsibleFullName,
                    searchCustomerShortName,
                    identityUserId
                }),
            columnDefs:
                [
                    {
                        title: l('Ticket:TicketStatus'),
                        width: "10%",
                        data: "ticketStatusDisplayName"
                    },

                    //{
                    //    title: l('Actions'),
                    //    width: "15%",
                    //    rowAction:
                    //    {
                    //        items:
                    //            [
                    //                {
                    //                    text: l('Ticket:Details'),
                    //                    visible: abp.auth.isGranted('Ticket.Edit'),
                    //                    action: function (data) {
                    //                        console.log(data.record);
                    //                        window.open('/Tickets/Details/' + data.record.id, "_blank");
                    //                    }
                    //                },
                    //                {
                    //                    text: l("Delete"),
                    //                    visible: abp.auth.isGranted('Ticket.Delete'),
                    //                    action: function (data) {
                    //                        ticketsAppService.deleteTicket(data.record.id)
                    //                            .then(function () {
                    //                                abp.notify.success(l('Global:Notify:Success'));
                    //                                dataTable.ajax.reload();
                    //                            });
                    //                    },
                    //                    confirmMessage: function (data) {
                    //                        return l('Global:Notify:DeleteConfirm', data.record.subject);
                    //                    }
                    //                }
                    //            ]
                    //    }
                    //},
                    {
                        title: l('Ticket:Number'),
                        width: "10%",
                        data: "code",
                        render: (data, _, b) => {
                            return `<a href="/Tickets/Details/${b.id}">${data}</a>`
                        }
                    },
                    {
                        title: l('Ticket:Subject'),
                        width: "10%",
                        data: "subject"
                    },
                    {
                        title: l('Ticket:CreationTime'),
                        data: "creationTime",
                        width: "10%",
                        dataFormat: "date"
                    },
                    //{
                    //    title: l('Ticket:LastModificationTime'),
                    //    data: "lastModificationTime",
                    //    width: "10%",
                    //    dataFormat: "date"
                    //},
                    {
                        title: l('Ticket:ClosedTime'),
                        data: "dueDate",
                        width: "10%",
                        dataFormat: "date"
                    },
                    //{
                    //    title: l('Ticket:CreatorFullName'),
                    //    width: "15%",
                    //    data: "creatorFullName"
                    //},
                    //{
                    //    title: l('Ticket:ResponsibleFullName'),
                    //    width: "10%",
                    //    data: "responsibleFullName"
                    //}
                ]
        })
    );
    $('#reset').click(function (e) {
        searchCreationTime = null;
        searchDueDate = null;
        searchCustomerShortName = null;
        searchResponsibleFullName = null;
        searchTicketStatusId = null;
        dataTable.ajax.reload();
    });


    $('#createTicketButton').click(function (e) {
        e.preventDefault();
        createTicketModal.open({ id: idForTenant });
    });

    $('#changePasswordButton').click(function (e) {
        e.preventDefault();
        changePasswordModal.open({ id: identityUserId });
    });
    changePasswordModal.onResult(function () {
        abp.notify.success('Пароль успешно изменён');
    });
    $('#editEmployeeButton').click(function (e) {
        e.preventDefault();
        let employeeId = $(this).attr("employee-id");
        editCustomerModal.open({ id: employeeId });
    });
    $('#deleteEmployeeButton').click(async function (e) {
        let employeeId = $(this).attr("employee-id");
        let confirmed = await abp.message.confirm('Вы действительно хотите удалить сотрудника');

        if (confirmed) {
            await kV.server.users.deleteUser(identityUserId);
            window.open('/Users', "_self");
        }
    });


    $('#addEvent').click(function (e) {
        e.preventDefault();
        let tenantId = $(this).attr("tenant-id");
        createEventModal.open({ tenantId: tenantId });
    });
    createEventModal.onResult(function () {
        abp.notify.success(l('Global:Notify:Success'));
    });
    createTicketModal.onResult(function () {
        window.location.reload();
    });
    editCustomerModal.onResult(function () {
        window.location.reload();
    });
    updateModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#avatar-img").on("click", () => {
        const input = $('#file-loader');
        input.click();
    })
});