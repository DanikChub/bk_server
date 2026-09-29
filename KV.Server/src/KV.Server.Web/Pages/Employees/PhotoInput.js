$(function () {
    $('#photo-input').change(function (e) {
        var file = e.target.files[0];
        var reader = new FileReader();
        reader.onload = function (event) {
            $('#current-profile-photo').attr('src', event.target.result);
        };
        reader.readAsDataURL(file);
    });
});