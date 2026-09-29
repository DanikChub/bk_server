
abp.modals.EditTicketDescription = function () {

    function initModal(modalManager, args) {
        var $modal = modalManager.getModal();
        var $form = modalManager.getForm();
        new SummerNote("#UpdateTicket_Description");
    };

    return {
        initModal: initModal
    };
};