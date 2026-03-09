using System;

namespace TabsNotesThings.ViewModels.Parsers;

public class FretParser
{
    public static FretParser Instance { get; } = new();
    
    /// <summary>
    /// Парсит с начала строки до конца строки или whitespace
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public (int? fret, int len) Parse(ReadOnlySpan<char> span)
    {
        int len = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (char.IsDigit(span[i]))
            {
                len++;
            }
            else
            {
                break;
            }
        }

        if (len == 0)
        {
            return (null, 0);
        }

        return (int.Parse(span.Slice(0, len)), len);
    }
}