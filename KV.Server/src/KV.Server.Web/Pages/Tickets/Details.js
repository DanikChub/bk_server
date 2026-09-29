$(function () {
    var l = abp.localization.getResource('Server');
    var giveTicketModal = new abp.ModalManager(
        abp.appPath + 'Tickets/GiveTicketModal'
    );
    var createTicketTagModal = new abp.ModalManager(
        abp.appPath + 'Tickets/CreateTicketTagModal'
    );
    var writeMessageModal = new abp.ModalManager(
        abp.appPath + 'Tickets/WriteMessagePreviewModal'
    );
    var editTicketHistoryModal = new abp.ModalManager(
        abp.appPath + 'Tickets/EditTicketHistoryModal'
    );
    var editTicketTypeModal = new abp.ModalManager(
        abp.appPath + 'Tickets/EditTicketTypeModal'
    );
    var editTicketSectionModal = new abp.ModalManager(
        abp.appPath + 'Tickets/EditTicketSectionModal'
    );
    var editTicketDescriptionModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'Tickets/EditTicketDescriptionModal',
        modalClass: 'EditTicketDescription',
    });
    var ticketId = document.getElementById(
        'CreateTicketHistory_TicketId'
    ).value;

    $('#NotPickUpButton').click(async function (e) {
        e.preventDefault();

        await kV.server.tickets.ticketHistory.createNotPickUpTicketHistory(ticketId);
        window.open('/tickets/details/' + ticketId, '_self');
    });
    $('#GiveTicketNewUser').click(function (e) {
        e.preventDefault();
        giveTicketModal.open({ TicketId: ticketId });
    });
    $('#add-favorit').click(async function (e) {
        e.preventDefault();
        await kV.server.ticketFavorite.setFavoriteSpecialist(ticketId);
        window.open('/tickets/details/' + ticketId, '_self');
    });
    $('#delete-favorit').click(async function (e) {
        e.preventDefault();
        await kV.server.ticketFavorite.deleteFavoriteSpecialist(ticketId);
        window.open('/tickets/details/' + ticketId, '_self');
    });
    $('#CreateNewTicketTag').click(function (e) {
        e.preventDefault();
        createTicketTagModal.open({ TicketId: ticketId });
    });

    $('#WriteNewMessageButton').click(function (e) {
        e.preventDefault();
        writeMessageModal.open({ id: ticketId });
    });

    $('#EditTicketTypeButton').click(function (e) {
        e.preventDefault();
        editTicketTypeModal.open({ TicketId: ticketId });
    });

    $('#EditTicketSectionButton').click(function (e) {
        e.preventDefault();
        editTicketSectionModal.open({ TicketId: ticketId });
    });
    $('#EditTicketDescriptionButton').click(function (e) {
        e.preventDefault();
        editTicketDescriptionModal.open({ id: ticketId });
    });

    $('.EditTicketHistoryButton').on('click', function () {
        let ticketHistoryId = $(this).attr('ticket-history-id');
        editTicketHistoryModal.open({ id: ticketHistoryId });
    });

    $('.deleteTicket').click(async function (e) {
        let ticketId = $(this).attr('ticket-id');
        let confirmed = await abp.message.confirm(
            'Вы действительно хотите удалить заявку'
        );

        if (confirmed) {
            await kV.server.tickets.tickets.deleteTicket(ticketId);
            window.open('/tickets', '_self');
        }
    });
    $('.DeleteTicketHistoryButton').click(async function (e) {
        let ticketHistoryId = $(this).attr('ticket-history-id');
        let confirmed = await abp.message.confirm(
            'Вы действительно хотите удалить сообщение'
        );

        if (confirmed) {
            await kV.server.tickets.tickets.deleteTicketHistoryRecordById(
                ticketHistoryId
            );
            window.open('/tickets/details/' + ticketId, '_self');
        }
    });
    giveTicketModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });
    editTicketDescriptionModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });
    writeMessageModal.onResult(function () {
        return window.open('/tickets/detailsMessage/' + ticketId);
    });

    editTicketTypeModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });

    editTicketSectionModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });

    createTicketTagModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });

    editTicketHistoryModal.onResult(function () {
        return window.open('/tickets/details/' + ticketId, '_self');
    });

    const deleteTag = async (ticketId, tagId) => {
        await kV.server.tags.tags.deleteTagInTicket(ticketId, tagId);
    };

    async function onClickDeleteTag(e) {
        const tag = e.currentTarget.parentElement;
        const tagId = tag.id;
        const ticketId = document.getElementById('ticket-id').innerText;
        await deleteTag(ticketId, tagId);
        tag.remove();
    }

    const tags = document.querySelectorAll('.edit-tickets__tag');
    tags.forEach((tag) => (tag.children[0].onclick = onClickDeleteTag));


    $('#right-nav-form').on('submit', async function (e) {
        e.preventDefault();

        const status = $(
            '#CreateRightMenu_TicketStatusId option:selected'
        ).text();

        if (
            status.toLowerCase().trim() === 'закрыта' &&
            ($('#tickets-details-input').val().trim() === '' && $('.note-editable').text().trim() === '')
        ) {
            document.getElementById('tickets-details-input').scrollIntoView();
            $('#tickets-details-input').css('border', '1px solid red')
            return;
        }

        const id = $('#ticket-id').text()
        const formData = new FormData(e.target)

        if (status.toLowerCase().trim() === 'закрыта') {
            document.getElementById('form-message-post').submit()
        }

        await fetch(`/Tickets/Details/${id}?handler=RightNavMenu`, { method: 'POST', body: formData })
    })
});
