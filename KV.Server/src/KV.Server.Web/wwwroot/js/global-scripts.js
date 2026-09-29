function getIconByFileExtension(extension)
{
    if (extension == ".docx" || extension == ".doc") {
        return "fa fa-file-word";
    }
    else if (extension == ".pdf") {
        return "fa fa-file-pdf";
    }
    else if (extension == ".xls" || extension == ".xlsx") {
        return "fas fa-file-excel";
    }
    else if (extension == ".pptx" || extension == ".ppt") {
        return "fas fa-file-powerpoint";
    }
    else if (extension == ".zip") {
        return "fas fa-file-archive";
    }
    else if (extension == ".rar") {
        return "far fa-file-archive";
    }
    else if (extension == ".txt") {
        return "fas fa-file-alt";
    }
    else if (extension == ".jpeg") {
        return "far fa-file-image";
    }
    else if (extension == ".png" || extension == ".tiff") {
        return "fas fa-file-image";
    }
    else if (extension == ".gif") {
        return "fas fa-file-video";
    }
    return "fa fa-file";
}