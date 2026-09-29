class SummerNote {
    #summernoteConfig = {
        placeholder: '',
        tabsize: 2,
        height: 153,
        focus: true,
        disableResizeEditor: false,
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['view', ['codeviesw', 'fullscreen']],
            ['mybutton', ['customPicture']],
        ],
        buttons: {
            customPicture: function () {
                const ui = $.summernote.ui;

                const button = ui.button({
                    contents: '<i class="note-icon-picture"></i>',
                    tooltip: 'picture',
                    click: function () {
                        $('#images').click();
                    },
                });

                return button.render();
            },
        },
    };

    constructor(selector) {
        this.selector = selector;
        $(selector).summernote(this.#summernoteConfig);

        $('.note-editor button[data-toggle="dropdown"]').each((_key, value) => {
            $(value).on('click', function () {
                $(this).attr('data-bs-toggle', 'dropdown');
                ata('id', 'dropdownMenu');
            });
        });
    }
}

// example below

// $(document).ready(function() {
//     new SummerNote('#summernote');
// });

// and need to add styles below

//<style>
//        .note-btn.dropdown-toggle:after {
//            content: none;
//        }
//</style>
