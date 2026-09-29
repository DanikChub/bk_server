namespace System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class Enumerable
{
    public static IEnumerable<SelectListItem> RenderToSelectList<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, string> displayFunc,
        Func<TSource, string> valueFunc)
    {
        foreach (var item in source)
        {
            var text = displayFunc(item);
            var value = valueFunc(item);

            yield return new SelectListItem
            {
                Text = text,
                Value = value
            };
        }
    }

    public static IEnumerable<SelectListItem> RenderToSelectList<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, string> displayFunc,
        Func<TSource, object> valueFunc)
    {
        foreach (var item in source)
        {
            var text = displayFunc(item);
            var value = valueFunc(item);

            yield return new SelectListItem
            {
                Text = text,
                Value = value.ToString()
            };
        }
    }
}
