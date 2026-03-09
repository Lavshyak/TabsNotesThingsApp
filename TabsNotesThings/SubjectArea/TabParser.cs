using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TabsNotesThings.ViewModels.Parsers;
using TabsNotesThings.ViewModels.ReadOnlyModels;

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

public class NoteEnumArr
{
    public static NoteEnumArr Instance { get; } = new();

    public IReadOnlyList<NoteEnum> Values { get; } =
    [
        NoteEnum.C, NoteEnum.CSharp,
        NoteEnum.D, NoteEnum.DSharp,
        NoteEnum.E,
        NoteEnum.F, NoteEnum.FSharp,
        NoteEnum.G, NoteEnum.GSharp,
        NoteEnum.A, NoteEnum.ASharp,
        NoteEnum.B
    ];

    public IReadOnlyDictionary<NoteEnum, int> NoteEnumToIdxDict { get; }
    public IReadOnlyDictionary<NoteEnum, int> NoteEnumToOffsetFromADict { get; }
    public IReadOnlyDictionary<int, NoteEnum> NoteOffsetToNoteEnumDict { get; }
    public NoteEnumArr()
    {
        NoteEnumToIdxDict = Values.Select(KeyValuePair.Create).ToFrozenDictionary();
        var aIdx = NoteEnumToIdxDict[NoteEnum.A];
        NoteEnumToOffsetFromADict = Values.Select(noteEnum =>
        {
            var noteIdx = NoteEnumToIdxDict[noteEnum];
            var offset = noteIdx - aIdx;
            return KeyValuePair.Create(noteEnum, offset);
        }).ToFrozenDictionary();
        NoteOffsetToNoteEnumDict = NoteEnumToOffsetFromADict.Select(kvp => KeyValuePair.Create(kvp.Value, kvp.Key))
            .ToFrozenDictionary();
    }
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
    
 
    public record ReadedNextColumnValue(int Value, int StartIdx, int Length);

    

    

    public record FromTabStringResult
    {
        public IReadOnlyList<NoteMayBeWithOctave?>? RootNotes { get; private init; } = null;
        public IReadOnlyList<FretNumberOrSpecialSymbol[]>? Columns { get; private init; } = null;
        [MemberNotNullWhen(false, nameof(RootNotes), nameof(Columns))]
        public bool IsError { get; private init; }

        public FromTabStringResult(bool isError)
        {
            IsError = isError;
        }

        public static FromTabStringResult FromError()
        {
            return  new FromTabStringResult(true);
        }
        
        public static FromTabStringResult FromResult(IReadOnlyList<NoteMayBeWithOctave?> rootNotes, IReadOnlyList<FretNumberOrSpecialSymbol[]> columns)
        {
            return new FromTabStringResult(false)
            {
                RootNotes = rootNotes,
                Columns = columns
            };
        }
    }

    private readonly TabRowParser _tabRowParser = TabRowParser.Instance;

    public FromTabStringResult FromTabString(string tab)
    {
        var rows = tab.Split('\n').ToImmutableArray();

        var rowsParseResults = rows.Select(r => _tabRowParser.ParseTabRow(r)).ToImmutableArray();

        if (rowsParseResults.Any(rpr => rpr.IsError))
        {
            return FromTabStringResult.FromError();
        }

        List<NoteMayBeWithOctave?> rootNotes = new();
        
        for (int i = 0; i < rows.Length; i++)
        {
            var rowParseResult = rowsParseResults[i];
            if (rowParseResult.IsError)
            {
                return FromTabStringResult.FromError();
            }
            if (rowParseResult.IsEmpty.Value)
            {
                rootNotes.Add(null);
            }
            else if(rowParseResult.IsValid)
            {
                rootNotes.Add(rowParseResult.RootInTabRow.NoteMayBeWithOctave);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        
        int[] nextFretIndexes = rowsParseResults.Select(_ => 0).ToArray();
        int?[] startShouldBeNotLess = rowsParseResults.Select(r => r.RootInTabRow == null ? (int?) null : r.RootInTabRow.StartIdx+r.RootInTabRow.Length).ToArray();
        List<FretNumberOrSpecialSymbol[]> columns = new();
        
        while (true)
        {
            var minStartIdx = int.MaxValue;
            for (int i = 0; i < rows.Length; i++)
            {
                var fretInfo = rowsParseResults[i].TryGetFret(nextFretIndexes[i]);
                if(fretInfo == null)
                    continue;
                
                if (fretInfo.StartIdx < minStartIdx)
                {
                    minStartIdx = fretInfo.StartIdx;
                }
            }

            if (startShouldBeNotLess.Where(x => x.HasValue).Any(x => minStartIdx < x))
            {
                return FromTabStringResult.FromError();
            }

            if (minStartIdx == int.MaxValue)
            {
                break;
            }
            
            FretNumberOrSpecialSymbol[] column = new FretNumberOrSpecialSymbol[rowsParseResults.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                var fretInfo = rowsParseResults[i].TryGetFret(nextFretIndexes[i]);
                if (fretInfo == null)
                    continue;

                if (fretInfo.StartIdx != minStartIdx)
                {
                    continue;
                }

                

                column[i] = new FretNumberOrSpecialSymbol(fretInfo.Fret, fretInfo.SpecialSymbol);
                nextFretIndexes[i] += 1;
                startShouldBeNotLess[i] = fretInfo.StartIdx + fretInfo.Length;
            }
            
            columns.Add(column);
        }

        return FromTabStringResult.FromResult(rootNotes, columns);
    }

    public record struct FretNumberOrSpecialSymbol(int? FretNumber, char? SpecialSymbol = null);

    /*public IReadOnlyList<NoteRow> FromTabStringToNoteRows(string tab)
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
    }*/
}