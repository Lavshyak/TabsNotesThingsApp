using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TabsNotesThings.ViewModels.Parsers;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels.Printers;

public class TabPrinter
{
    public static TabPrinter Instance { get; } = new();

    public void PrintFretRows(IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> rows, IReadOnlyList<StringBuilder> rowsSbs)
    {
        foreach (var row in rows)
        {
            string[] strs = row.Select(x =>
            {
                if (x.FretNumber != null)
                {
                    return x.FretNumber.Value.ToString();
                }
                else if (x.SpecialSymbol.HasValue)
                {
                    return x.SpecialSymbol.Value.ToString();
                }
                else
                {
                    return "·";
                }
            }).ToArray();
            var maxLen = strs.Max(s => s.Length);
            for (int i = 0; i < strs.Length; i++)
            {
                rowsSbs[i].Append(strs[i].PadRight(maxLen + 1));
            }
        }
    }

    public IReadOnlyList<string> PrintFretRows(IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> rows)
    {
        var rowsSbs = rows.Select(_ => new StringBuilder()).ToArray();

        PrintFretRows(rows, rowsSbs);

        var results = rowsSbs.Select(sb => sb.ToString()).ToArray();
        return results;
    }

    public void PrintNoteRows(IReadOnlyList<int?> indexesFromA4WhichAreRoots,
        IReadOnlyList<IReadOnlyList<int?>> columns, IReadOnlyList<StringBuilder> rowsSbs)
    {
        throw new NotImplementedException();
        /*foreach (var column in columns)
        {
            column.Select((fret, i) =>
            {

            });
            string[] strs = column.Select(x => x.HasValue ? x.Value.ToString() : "").ToArray();
            var maxLen = strs.Max(s => s.Length);
            for (int i = 0; i < strs.Length; i++)
            {
                rowsSbs[i].Append(strs[i].PadRight(maxLen+1));
            }
        }*/
    }

    public void PrintStrings(IReadOnlyList<string> rows, IReadOnlyList<StringBuilder> rowsSbs)
    {
        var maxLen = rows.Max(s => s.Length);
        for (int i = 0; i < rows.Count; i++)
        {
            rowsSbs[i].Append(rows[i].PadRight(maxLen + 1));
        }
    }

    public string PrintTabRootNotesFretNumbers(TabViewModel.Tab3 tab)
    {
        var rowsSbs = tab.Roots.Select(_ => new StringBuilder()).ToImmutableArray();
        
        var rootStrs =
            tab.Roots.Select(r => r != null ? NoteStrs.Instance.ToStringNoteOctave(r.NoteEnum, r.Octave) : "·").ToImmutableArray();
        PrintStrings(rootStrs, rowsSbs);
        
        PrintFretRows(tab.Columns, rowsSbs);

        var result = string.Join("\n", rowsSbs.Select(rsb => rsb.ToString()));
        return result;
    }

    public void PrintFretColumnToNoteColumn(IReadOnlyList<int?> rootIdxes, IReadOnlyList<TabParser.FretNumberOrSpecialSymbol> fretColumn, IReadOnlyList<StringBuilder> rowsSbs)
    {
        List<string> strs = [];
        for (int i = 0; i < rootIdxes.Count; i++)
        {
            var rootIdx = rootIdxes[i];
            if (!rootIdx.HasValue)
            {
                strs.Add("·");
            }
            else
            {
                var fret = fretColumn[i];
                if (fret.SpecialSymbol.HasValue)
                {
                    strs.Add(fret.SpecialSymbol.Value.ToString());
                }
                else if (fret.FretNumber.HasValue)
                {
                    var noteIdx = rootIdxes[i].Value+fretColumn[i].FretNumber.Value;
                    var noteAndOctave = NoteStrs.Instance.ToNoteAndOctave(noteIdx);
                    var str = NoteStrs.Instance.ToStringNoteOctave(noteAndOctave.note, noteAndOctave.octave);
                    strs.Add(str);
                }
                else
                {
                    strs.Add("·");
                }
            }
        }

        PrintStrings(strs, rowsSbs);
    }

    public void PrintFretColumns(IReadOnlyList<int?> rootIdxes, IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> fretColumns, IReadOnlyList<StringBuilder> rowsSbs)
    {
        foreach (var column in fretColumns)
        {
            PrintFretColumnToNoteColumn(rootIdxes, column, rowsSbs);
        }
    }
    
    public void PrintFretColumns(IReadOnlyList<NoteWithOctave?> roots, IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> fretColumns, IReadOnlyList<StringBuilder> rowsSbs)
    {
        var rootIdxes = roots.Select(r => r != null ? NoteStrs.Instance.ToHalfToneIdxRelativeToA4(r) : (int?)null).ToImmutableArray();
        foreach (var column in fretColumns)
        {
            PrintFretColumnToNoteColumn(rootIdxes, column, rowsSbs);
        }
    }
    
    public string PrintTabRootNotesFretNotes(TabViewModel.Tab3 tab)
    {
        var rowsSbs = tab.Roots.Select(_ => new StringBuilder()).ToImmutableArray();
        
        var rootStrs =
            tab.Roots.Select(r => r != null ? NoteStrs.Instance.ToStringNoteOctave(r.NoteEnum, r.Octave) : "·").ToImmutableArray();
        PrintStrings(rootStrs, rowsSbs);
        PrintStrings(Enumerable.Repeat(" ", rowsSbs.Length).ToImmutableArray(), rowsSbs);
        
        PrintFretColumns(tab.Roots, tab.Columns, rowsSbs);

        var result = string.Join("\n", rowsSbs.Select(rsb => rsb.ToString()));
        return result;
    }
}