using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels.Parsers;

public class TabRowParser
{
    public static TabRowParser Instance { get; } = new TabRowParser();
    
    private NoteOrFretParser _noteOrFretParser = NoteOrFretParser.Instance;
    
    public record FretInTabRow(int StartIdx, int Length, int? Fret, char? SpecialSymbol = null);

    public record RootInTabRow(int StartIdx, int Length, NoteMayBeWithOctave? NoteMayBeWithOctave);
    
    public record ParseTabRowResult
    {
        public RootInTabRow? RootInTabRow { get; private init; } = null;
        public IReadOnlyList<FretInTabRow>? FretsInTabRow { get; private init; } = null;
        [MemberNotNullWhen(false, nameof(IsEmpty))]
        public bool IsError { get; private init; }
        /// <summary>
        /// Is valid
        /// </summary>
        [MemberNotNullWhen(false, nameof(RootInTabRow), nameof(FretsInTabRow))]
        public bool? IsEmpty { get; private init; } = null;
        /// <summary>
        /// Not empty
        /// </summary>
        [MemberNotNullWhen(true, nameof(RootInTabRow), nameof(FretsInTabRow))]
        public bool IsValid { get; private init; } = false;
        
        private ParseTabRowResult(bool isError)
        {
            IsError = isError;
        }

        public static ParseTabRowResult FromResult(RootInTabRow rootInTabRow, IReadOnlyList<FretInTabRow> fretsInTabRow)
        {
            return new ParseTabRowResult(false)
            {
                RootInTabRow = rootInTabRow,
                FretsInTabRow = fretsInTabRow,
                IsEmpty = false,
                IsValid = true
            };
        }
        
        public static ParseTabRowResult FromError()
        {
            return new ParseTabRowResult(true);
        }
        
        public static ParseTabRowResult FromEmpty()
        {
            return new ParseTabRowResult(false)
            {
                IsEmpty = true
            };
        }

        public FretInTabRow? TryGetFret(int idx)
        {
            if (!this.IsValid)
            {
                return null;
            }

            if (idx >= this.FretsInTabRow.Count)
            {
                return null;
            }
            
            return this.FretsInTabRow[idx];
        }
    };

    public ParseTabRowResult ParseTabRow(string row)
    {
        RootInTabRow? rootInTabRow = null;
        int nextStartIdx = 0;
        {
            var noteParseResult = _noteOrFretParser.ParseNextNoteOrFret(row.AsSpan(nextStartIdx));
            if (noteParseResult.IsError)
            {
                return ParseTabRowResult.FromError();
            }

            if (noteParseResult.IsEmpty.Value)
            {
                return ParseTabRowResult.FromEmpty();
            }

            if (!noteParseResult.IsValidNote)
            {
                return ParseTabRowResult.FromError();
            }

            rootInTabRow = new RootInTabRow(nextStartIdx, noteParseResult.Length.Value,
                new NoteMayBeWithOctave(noteParseResult.NoteEnum.Value, noteParseResult.Octave));

            nextStartIdx = nextStartIdx + noteParseResult.StartIdx.Value + noteParseResult.Length.Value;
        }

        var fretsInTabRow = new List<FretInTabRow>();

        while (true)
        {
            var noteParseResult = _noteOrFretParser.ParseNextNoteOrFret(row.AsSpan(nextStartIdx));
            if (noteParseResult.IsError)
            {
                return ParseTabRowResult.FromError();
            }

            if (noteParseResult.IsEmpty.Value)
            {
                break;
            }

            if (noteParseResult.IsValidFret)
            {
                fretsInTabRow.Add(new FretInTabRow(nextStartIdx+noteParseResult.StartIdx.Value, noteParseResult.Length.Value, noteParseResult.Fret.Value, null));
            }
            else if(noteParseResult.SpecialSymbol.HasValue)
            {
                fretsInTabRow.Add(new FretInTabRow(nextStartIdx+noteParseResult.StartIdx.Value, noteParseResult.Length.Value, null, noteParseResult.SpecialSymbol));
            }
            else
            {
                throw new InvalidOperationException();
            }
            
            nextStartIdx = nextStartIdx + noteParseResult.StartIdx.Value + noteParseResult.Length.Value;
        }

        return ParseTabRowResult.FromResult(rootInTabRow, fretsInTabRow);
    }

}