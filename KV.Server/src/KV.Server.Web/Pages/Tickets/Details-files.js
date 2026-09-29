$(document).ready(function () {
    var summerNote = new SummerNote('#summernote');
    $("#response-template").on("change", function () {
        let templateText = $("#response-template").val();
        if (templateText != null && templateText != "") {
            let text = summerNote.getText();
            if (text != "" && text != null) {
                templateText += "<br />";
            }
            summerNote.setText(templateText + text);
        }
    });

    const fileArr = [];

    $("#images").change(function () {
        const total_file = this.files;

        if (!total_file.length) return;

        Array.from(total_file).forEach(file => {
            if (!fileArr.some(existingFile => existingFile.name === file.name)) {
                fileArr.push(file);
            }
        });

        drawFiles(fileArr);
        updateInputFiles();
    });

    function drawFiles(fileList) {
        const filesImgsContainer = $('#image_preview');
        filesImgsContainer.html("");

        fileArr.forEach(file => {
            const { name } = file;
            const extension = "." + name.split('.').pop();

            filesImgsContainer.append(`
                    <div onclick="removeFileFromSummerNote('${name}')" class="file-box-item">
                        <div data-toggle="tooltip" title="${name}" class="file-box-item__file-name">
                            ${name}
                        </div>
                        <div class="file-box-item__img">
                            <div class="${chatIcon(extension)}"></div>
                        </div>
                        <div class="file-box-item__close hideCloseButton">
                            <i class="fa fa-times" aria-hidden="true"></i>
                        </div>
                    </div>`);
        });
    }

    function updateInputFiles() {
        const dataTransfer = new DataTransfer();
        fileArr.forEach(file => dataTransfer.items.add(file));

        const inputFile = document.querySelector('#images');
        inputFile.files = dataTransfer.files;
    }

    window.removeFileFromSummerNote = function (filename) {
        const index = fileArr.findIndex(file => file.name === filename);
        if (index !== -1) {
            fileArr.splice(index, 1);
            drawFiles(fileArr);
            updateInputFiles();
        }
    };

    $('#save-btn').on('click', () => {
        abp.message.success('', 'Изменения сохранены');
        setTimeout(() => window.location.reload(), 2000);
    });
})