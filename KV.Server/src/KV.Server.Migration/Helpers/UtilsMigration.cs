namespace KV.Server.Migration.Helpers;

using System;
using System.Text.RegularExpressions;

public static partial class UtilsMigration
{
    /// <summary>
    /// У файла в DisplayName есть - " от 12.12.2022 12:12", он не даёт получить оригинальное названия файла
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="Created"></param>
    /// <returns></returns>
    public static string MediaFileSplitDisplayName(string displayName, DateTime Created)
    {
        var splitSring = " от ";
        var piece = displayName.Split(splitSring);
        if (piece.Length == 0)
        {
            return displayName;
        }

        var result = "";
        for (var i = 0; i < piece.Length - 1; i++)
        {
            result += piece[i];
        }
        return result;
    }

    /// <summary>
    /// Имя, Фамилия, Отчество бывают вместе в одной колонки и некоторые null, но вместе они дают ФИО
    /// </summary>
    /// <param name="lastName"></param>
    /// <param name="firstName"></param>
    /// <param name="middleName"></param>
    /// <returns></returns>
    public static (string LastName, string FirstName, string MiddleName) HdProfileFixFirstLastMiddleName(string FirstName, string LastName, string MiddleName)
    {
        var fullName = $"{LastName ?? ""} {FirstName ?? ""} {MiddleName ?? ""}";
        var result = (LastName, FirstName, MiddleName);
        try
        {
            result = GetFioByFullName(fullName);
        }
        catch { }

        return result;
    }

    /// <summary>
    /// Если строка null или состоит только из whitespace вернуть null
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetNullIfNullOrWhiteSpace(string str) => str.IsNullOrWhiteSpace() ? null : str;

    /// <summary>
    /// Полное имя в отдельные части ФИО
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public static (string LastName, string FirstName, string MiddleName) GetFioByFullName(string fullName)
    {
        fullName = MyRegex().Replace(fullName, " "); // удаляет n-whitespace
        var fioPiece = fullName.Replace(".", ". ").Trim().Split(' ');
        if (fioPiece.Length < 3)
        {
            throw new Exception($"\"{fullName}\" - fullname has less than 3 names");
        }
        else if (fioPiece.Length > 3)
        {
            throw new Exception($"\"{fullName}\" - fullname has more than 3 names");
        }
        var last = GetNullIfNullOrWhiteSpace(fioPiece[0]);
        var first = GetNullIfNullOrWhiteSpace(fioPiece[1]);
        var middle = GetNullIfNullOrWhiteSpace(fioPiece[2]);
        return (last, first, middle);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex MyRegex();
}
