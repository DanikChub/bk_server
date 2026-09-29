
abp.modals.CreateContract = function () {

    function initModal(modalManager, args) {
        var $modal = modalManager.getModal();
        var $form = modalManager.getForm();
    };

    return {
        initModal: initModal
    };
};