namespace KV.Server.Helpers;

public static class GlobalClass
{
    public static string GetIconByFileExtension(string extension)
    {
        if (extension is ".docx" or ".doc")
        {
            return "file-word"; //fa fa-file-word
        }

        if (extension == ".pdf")
        {
            return "file-pdf"; //fa fa-file-pdf
        }

        if (extension is ".xls" or ".xlsx")
        {
            return "file-xls"; //fas fa-file-excel
        }
        // else if (extension == ".pptx" || extension == ".ppt")
        // {
        //     return "fas fa-file-powerpoint";
        // }

        if (extension == ".zip")
        {
            return "file-zip"; //fas fa-file-archive
        }

        // else if (extension == ".rar")
        // {
        //     return "far fa-file-archive";
        // }
        // else if (extension == ".txt")
        // {
        //     return "fas fa-file-alt";
        // }
        // else if (extension == ".jpeg")
        // {
        //     return "far fa-file-image";
        // }
        // else if (extension == ".png" || extension == ".tiff")
        // {
        //     return "fas fa-file-image";
        // }
        // else if (extension == ".gif")
        // {
        //     return "fas fa-file-video";
        // }
        return "file-another"; //fa fa-file
    }
}
