// $(async function () {
//     var l = abp.localization.getResource('Server');
//     const customerId = $('#Customer_Id').attr('data-customer-id');

//     var updateModal = new abp.ModalManager(abp.appPath + 'Tickets/EditModal');

//     var ticketsAppService = kV.server.tickets.tickets;

//     let urlParams = new URLSearchParams(window.location.search);
//     let searchCreationTime = urlParams.get('Search.TicketCreationTime');
//     let searchDueDate = urlParams.get('Search.TicketDueDate');
//     let searchResponsibleFullName = urlParams.get(
//         'Search.TicketResposibleFullName'
//     );
//     let searchCreatorFullName = urlParams.get('Search.TicketCreatorFullName');
//     let searchTicketStatusId = urlParams.get('Search.TicketStatusId');
//     document.getElementById('Search_TicketCreationTime').value =
//         searchCreationTime;
//     document.getElementById('Search_TicketDueDate').value = searchDueDate;
//     document.getElementById('Search_TicketResposibleFullName').value =
//         searchResponsibleFullName;
//     document.getElementById('Search_TicketCreatorFullName').value =
//         searchCreatorFullName;
//     document.getElementById('Search_TicketStatusId').value =
//         searchTicketStatusId;

//     var dataTable = $('#TicketsTable').DataTable(
//         abp.libs.datatables.normalizeConfiguration({
//             serverSide: true,
//             paging: true,
//             order: [[1, 'asc']],
//             searching: false,
//             scrollX: true,
//             ajax: abp.libs.datatables.createAjax(
//                 kV.server.tickets.tickets.getTicketsList,
//                 {
//                     searchTicketStatusId: searchTicketStatusId,
//                     searchCreatorFullName: searchCreatorFullName,
//                     searchResponsibleFullName: searchResponsibleFullName,
//                     searchCreationTime: searchCreationTime,
//                     searchDueDate: searchDueDate,
//                     tenantId: customerId,
//                 }
//             ),
//             columnDefs: [
//                 {
//                     title: l('Actions'),
//                     rowAction: {
//                         items: [
//                             {
//                                 text: l('Ticket:Details'),
//                                 visible: abp.auth.isGranted('Ticket.Edit'),
//                                 action: function (data) {
//                                     console.log(data.record);
//                                     window.open(
//                                         '/Tickets/Details/' + data.record.id,
//                                         '_blank'
//                                     );
//                                 },
//                             },
//                             {
//                                 text: l('Delete'),
//                                 visible: abp.auth.isGranted('Ticket.Delete'),
//                                 action: function (data) {
//                                     ticketsAppService
//                                         .deleteTicket(data.record.id)
//                                         .then(function () {
//                                             abp.notify.success(
//                                                 l('Global:Notify:Success')
//                                             );
//                                             dataTable.ajax.reload();
//                                         });
//                                 },
//                                 confirmMessage: function (data) {
//                                     return l(
//                                         'Global:Notify:DeleteConfirm',
//                                         data.record.subject
//                                     );
//                                 },
//                             },
//                         ],
//                     },
//                 },
//                 {
//                     title: l('Ticket:Code'),
//                     data: 'code',
//                 },
//                 {
//                     title: l('Ticket:Subject'),
//                     data: 'subject',
//                 },
//                 {
//                     title: l('Ticket:TicketStatus'),
//                     data: 'ticketStatusDisplayName',
//                 },
//                 {
//                     title: l('Ticket:StartTime'),
//                     data: 'creationTime',
//                     dataFormat: 'date',
//                 },
//                 {
//                     title: l('Ticket:LastModificationTime'),
//                     data: 'lastModificationTime',
//                     dataFormat: 'date',
//                 },
//                 {
//                     title: l('Ticket:EndTime'),
//                     data: 'dueDate',
//                     dataFormat: 'date',
//                 },
//                 {
//                     title: l('Ticket:CreatorFullName'),
//                     data: 'creatorFullName',
//                 },
//                 {
//                     title: l('Ticket:ResponsibleFullName'),
//                     data: 'responsibleFullName',
//                 },
//             ],
//         })
//     );
//     $('#reset').click(function (e) {
//         searchCreationTime = null;
//         searchDueDate = null;
//         searchResponsibleFullName = null;
//         searchCreatorFullName = null;
//         searchTicketStatusId = null;
//         dataTable.ajax.reload();
//     });
//     updateModal.onResult(function () {
//         dataTable.ajax.reload();
//     });
// });
