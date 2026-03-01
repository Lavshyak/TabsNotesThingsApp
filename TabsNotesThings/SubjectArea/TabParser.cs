using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace TabsNotesThings.ViewModels;

public enum NoteEnum
{
    C,
    CSharp,
    D,
    DSharp,
    E,
    F,
    FSharp,
    G,
    GSharp,
    A,
    ASharp,
    B
}

public class NoteNames
{
    public static NoteNames Instance { get; } = new NoteNames();
    
    public string C { get; } = "C";
    public string CSharp { get; } = "C#";
    public string D { get; } = "D";
    public string DSharp { get; } = "D#";
    public string E { get; } = "E";
    public string F { get; } = "F";
    public string FSharp { get; } = "F#";
    public string G { get; } = "G";
    public string GSharp { get; } = "G#";
    public string A { get; } = "A";
    public string ASharp { get; } = "A#";
    public string B { get; } = "B";
}

public class NotesEnumToStrings
{
    public static NotesEnumToStrings Instance { get; } = new(); 
    
    public IReadOnlyDictionary<string, NoteEnum> NoteNameToNoteEnumDict { get; }
    public IReadOnlyDictionary<string, NoteEnum> LowerNoteNameToNoteEnumDict { get; }
    public IReadOnlyDictionary<NoteEnum, string> NoteEnumToNoteNameDict { get; }

    public NotesEnumToStrings()
    {
        var noteNames = NoteNames.Instance;
        NoteNameToNoteEnumDict = new Dictionary<string, NoteEnum>()
        {
            { noteNames.C, NoteEnum.C },
            { noteNames.CSharp, NoteEnum.CSharp },
            { noteNames.D, NoteEnum.D },
            { noteNames.DSharp, NoteEnum.DSharp },
            { noteNames.E, NoteEnum.E },
            { noteNames.F, NoteEnum.F },
            { noteNames.FSharp, NoteEnum.FSharp },
            { noteNames.G, NoteEnum.G },
            { noteNames.GSharp, NoteEnum.GSharp },
            { noteNames.A, NoteEnum.A },
            { noteNames.ASharp, NoteEnum.ASharp },
            { noteNames.B, NoteEnum.B },
        }.ToFrozenDictionary();

        LowerNoteNameToNoteEnumDict = NoteNameToNoteEnumDict
            .Select(kvp => KeyValuePair.Create(kvp.Key.ToLower(), kvp.Value))
            .ToFrozenDictionary();
        NoteEnumToNoteNameDict = NoteNameToNoteEnumDict.Select(kvp => KeyValuePair.Create(kvp.Value, kvp.Key))
            .ToFrozenDictionary();
    }

    public string EnumToName(NoteEnum noteEnum)
    {
        return NoteEnumToNoteNameDict[noteEnum];
    }

    public NoteEnum AnyNoteNameToEnum(string anyNoteName)
    {
        return LowerNoteNameToNoteEnumDict[anyNoteName.ToLower()];
    }
}

public class TabParser
{
    public static TabParser Instance { get; } = new(); 
    
    public record ParseNoteResult(NoteEnum? NoteEnum, int? Octave, int? StartIdx, int? Length, bool IsError);

    public ParseNoteResult ParseNote(string noteStringFromTab)
    {
        if (string.IsNullOrWhiteSpace(noteStringFromTab))
            return new(null, null, null, null, true);

        var s = noteStringFromTab;
        int i = 0;

        while (i < s.Length && s[i] == ' ')
            i++;

        if (i >= s.Length)
            return new(null, null, null, null, true);

        int start = i;

        char c = s[i];
        if (!char.IsLetter(c))
            return new(null, null, null, null, true);

        c = char.ToLowerInvariant(c);
        if (c < 'a' || c > 'g')
            return new(null, null, null, null, true);

        i++;

        // optional #
        if (i < s.Length && s[i] == '#')
            i++;

        var noteName = s.Substring(start, i - start);

        if (!NotesEnumToStrings.Instance.LowerNoteNameToNoteEnumDict
                .TryGetValue(noteName.ToLowerInvariant(), out var noteEnum))
            return new(null, null, null, null, true);

        int? octave = null;

        if (i < s.Length && char.IsDigit(s[i]))
        {
            octave = s[i] - '0';
            i++;
        }

        int length = i - start;

        return new(noteEnum, octave, start, length, false);
    }

    public ParseNoteResult ParseNote(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return new(null, null, null, null, true);

        int i = 0;

        // skip spaces
        while (i < span.Length && span[i] == ' ')
            i++;

        if (i >= span.Length)
            return new(null, null, null, null, true);

        int start = i;
        char c = span[i];

        // Case 1: digit -> octave only
        if (char.IsDigit(c))
        {
            int octave = c - '0';
            return new(null, octave, start, null, false);
        }

        // Case 2: note
        if (!char.IsLetter(c))
            return new(null, null, null, null, true);

        char lc = char.ToLowerInvariant(c);
        if (lc < 'a' || lc > 'g')
            return new(null, null, null, null, true);

        i++;

        // optional #
        if (i < span.Length && span[i] == '#')
            i++;

        var noteName = span.Slice(start, i - start).ToString();

        if (!NotesEnumToStrings.Instance.LowerNoteNameToNoteEnumDict
                .TryGetValue(noteName.ToLowerInvariant(), out var noteEnum))
            return new(null, null, null, null, true);

        int? octaveOpt = null;

        // optional octave digit
        if (i < span.Length && char.IsDigit(span[i]))
        {
            octaveOpt = span[i] - '0';
            i++;
        }

        int length = i - start;
        return new(noteEnum, octaveOpt, start, length, false);
    }
    
    public void FromTabString(string tab,
        out List<string> rowRootKeys,
        out List<List<int?>> rows)
    {
        rowRootKeys = [];
        rows = [];

        var strRows = tab.Split("\n\r".ToCharArray()).Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();
        var maxLen = strRows.Max(r => r.Length);
        for (int i = 0; i < strRows.Length; i++)
        {
            strRows[i] = strRows[i].PadRight(maxLen, ' ');
        }

        int columnCount = strRows.First().Length;
        int rowsCount = strRows.Length;
        rows.AddRange(Enumerable.Range(0, rowsCount).Select(_ => new List<int?>(1000)));
        //rowRootKeys.AddRange(Enumerable.Range(0, rowsCount).Select(_ => (string?)null)!);
        bool keysParsed = false;

        string parseArea(int columnIdx, int row)
        {
            var nextSpaceIdx = strRows[row].IndexOf(' ', columnIdx);
            if (nextSpaceIdx == -1)
            {
                return strRows[row].Substring(columnIdx);
            }
            else
            {
                return strRows[row].Substring(columnIdx, nextSpaceIdx - columnIdx);
            }
        }

        for (int columnIdx = 0; columnIdx < columnCount;)
        {
            int columnIdxIncrease = 1;
            bool willKeysBeParsed = false;
            for (int rowIdx = 0; rowIdx < rowsCount; rowIdx++)
            {
                if (strRows[rowIdx][columnIdx] == ' ')
                {
                    continue;
                }
                else
                {
                    var parsedAreaStr = parseArea(columnIdx, rowIdx);
                    columnIdxIncrease = Math.Max(columnIdxIncrease, parsedAreaStr.Length);
                    if (!keysParsed)
                    {
                        rowRootKeys.Add(parsedAreaStr);
                        willKeysBeParsed = true;
                    }
                    else
                    {
                        var parsedFretNumber = int.Parse(parsedAreaStr);
                        rows[rowIdx].Add(parsedFretNumber);
                    }
                }
            }

            var len = rows.Max(r => r.Count);
            foreach (var row in rows)
            {
                while (row.Count < len)
                {
                    row.Add(null);
                }
            }

            if (willKeysBeParsed)
            {
                keysParsed = true;
            }

            columnIdx += columnIdxIncrease;
        }
    }

    public IReadOnlyList<NoteRow> FromTabStringToNoteRows(string tab)
    {
        FromTabString(tab, out var rowRootKeys, out var rows);
        var rowRootNotes = rowRootKeys.Select(rrk => Note.AllNotes.First(n => n.Key == rrk && n.Octave == 4)).ToArray();
        var rs = rowRootNotes.Zip(rows).Select(x =>
            new NoteRow(
                x.First,
                x.Second.Select(diff =>
                    diff == null ? null : RelativeNote.FromRootNote(x.First, diff.Value)
                ).ToArray()
            )
        );
        return rs.ToArray();
    }
}