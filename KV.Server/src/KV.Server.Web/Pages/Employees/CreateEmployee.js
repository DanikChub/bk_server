$(function () {
    $(document).ready(function () {
        $('.select2').select2();
    });

    $('#new-employee').on('submit', function () {
        if ($('#EmployeeVM_RoleNames').hasClass('input-validation-error')) {
            $('span.select2').css('border', '1px solid red');
        } else {
            $('span.select2').css('border', '0');
        }
    });
});
