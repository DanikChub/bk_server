namespace KV.Server.Helpers;
using System.Linq;
using System.Text.RegularExpressions;

public static partial class GetFullNameByString
{
    /// <summary>
    ///     Полное имя в отдельные части ФИО
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public static (string LastName, string FirstName, string MiddleName) GetFioByFullName(string fullName)
    {
        if (fullName == null)
        {
            return (null, null, null);
        }

        fullName = fullName.Replace(".", ". ");
        fullName = MyRegex().Replace(fullName, " "); // удаляет n-whitespace
        var fioPiece = fullName.Trim().Split(' ');
        var last = fioPiece.FirstOrDefault();
        var first = string.Empty;
        var middle = string.Empty;
        if (fioPiece.Length == 2)
        {
            first = fioPiece.LastOrDefault();
        }
        else if (fioPiece.Length == 3)
        {
            first = fioPiece[1];
            middle = fioPiece[2];
        }
        else if (fioPiece.Length > 3)
        {
            first = fioPiece[1];
            for (var i = 2; i < fioPiece.Length - 2; i++)
            {
                middle += fioPiece[i];
            }
        }

        return (last, first, middle);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex MyRegex();
}
