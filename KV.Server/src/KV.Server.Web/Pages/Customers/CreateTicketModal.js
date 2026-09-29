abp.modals.CreateTicketModal = function () {

    function initModal(modalManager, args) {
        var $modal = modalManager.getModal();
        var $form = modalManager.getForm();
        new SummerNote("#CreateTicket_Description");
    };

    return {
        initModal: initModal
    };
};