const chatIcon = (format) => {
    let imageIcon;

    switch (format) {
        case '.pdf':
            imageIcon = "file-pdf"
            break;
        case '.docx':
            imageIcon = "file-word"
            break;
        case '.zip':
            imageIcon = "file-zip"
            break;
        case '.xls':
            imageIcon = "file-xls"
            break;
        default:
            imageIcon = "file-another"
    };

    return imageIcon;
};