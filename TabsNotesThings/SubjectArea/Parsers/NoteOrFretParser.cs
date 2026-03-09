using System;
using System.Diagnostics.CodeAnalysis;

namespace TabsNotesThings.ViewModels.Parsers;

public class NoteOrFretParser
{
    public static NoteOrFretParser Instance { get; } = new();
    
    public class ParseNoteOrFretResult
    {
        public NoteEnum? NoteEnum { get; private init; } = null;
        public int? Octave { get; private init; } = null;
        public int? Fret { get; private init; } = null;
        public int? StartIdx { get; private init; } = null;
        public int? Length { get; private init; } = null;
        [MemberNotNullWhen(false, nameof(StartIdx), nameof(Length))]
        public bool? IsEmpty { get; private init; } = null;
        [MemberNotNullWhen(false, nameof(IsEmpty))]
        public bool IsError { get; private init; }
        [MemberNotNullWhen(true, nameof(NoteEnum), nameof(StartIdx), nameof(Length))]
        public bool IsValidNote { get; private init; } = false;
        [MemberNotNullWhen(true, nameof(Fret), nameof(StartIdx), nameof(Length))]
        public bool IsValidFret { get; private init; } = false;

        private ParseNoteOrFretResult(bool isError)
        {
            IsError = isError;
        }
        
        public static ParseNoteOrFretResult FromFret(int fret, int startIdx, int length)
        {
            if (fret < 0 || startIdx < 0 || length < 0)
            {
                throw new ArgumentException();
            }
            
            return new ParseNoteOrFretResult(false)
            {
                Fret = fret,
                StartIdx = startIdx,
                Length = length,
                IsEmpty = false,
                IsValidFret = true
            };
        }
        
        public static ParseNoteOrFretResult FromNote(NoteEnum noteEnum, int? octave, int startIdx, int length)
        {
            if (startIdx < 0 || length < 0)
            {
                throw new ArgumentException();
            }
            
            return new ParseNoteOrFretResult(false)
            {
                NoteEnum = noteEnum,
                Octave = octave,
                StartIdx = startIdx,
                Length = length,
                IsEmpty = false,
                IsValidNote = true
            };
        }
        
        public static ParseNoteOrFretResult FromError()
        {
            return new ParseNoteOrFretResult(true);
        }

        public static ParseNoteOrFretResult FromEmpty()
        {
            return new  ParseNoteOrFretResult(false)
            {
                IsEmpty = true
            };
        }
    }
    
    public ParseNoteOrFretResult ParseNextNoteOrFret(string noteStringFromTab)
    {
        return ParseNextNoteOrFret(noteStringFromTab.AsSpan());
    }

    public ParseNoteOrFretResult ParseNextNoteOrFret(string noteStringFromTab, int startIdx)
    {
        return ParseNextNoteOrFret(noteStringFromTab.AsSpan(startIdx));
    }

    private readonly FretParser _fretParser = FretParser.Instance; 
    
    public ParseNoteOrFretResult ParseNextNoteOrFret(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return ParseNoteOrFretResult.FromEmpty();

        int i = 0;

        // skip spaces
        while (i < span.Length && char.IsWhiteSpace(span[i]))
            i++;

        if (i >= span.Length)
            return ParseNoteOrFretResult.FromEmpty();

        int start = i;
        char c = span[i];

        // Case 1: digit -> fret only
        if (char.IsDigit(c))
        {
            (int? fret, int len) = _fretParser.Parse(span.Slice(i));
            if (!fret.HasValue)
                throw new InvalidOperationException();

            i += len;

            if ((span.Length > i && char.IsWhiteSpace(span[i])) || span.Length == i)
            {
                return ParseNoteOrFretResult.FromFret(fret.Value, start, len);
            }
            else
            {
                return ParseNoteOrFretResult.FromError();
            }
        }

        // Case 2: note
        if (!char.IsLetter(c))
            return ParseNoteOrFretResult.FromError();

        char lc = char.ToLowerInvariant(c);
        if (lc < 'a' || lc > 'g')
            return ParseNoteOrFretResult.FromError();

        i++;

        // optional #
        if (i < span.Length && span[i] == '#')
            i++;

        var noteName = span.Slice(start, i - start).ToString();

        if (!NotesEnumToStrings.Instance.LowerNoteNameToNoteEnumDict
                .TryGetValue(noteName.ToLowerInvariant(), out var noteEnum))
            return ParseNoteOrFretResult.FromError();

        int? octaveOpt = null;

        // optional octave digit
        if (i < span.Length && char.IsDigit(span[i]))
        {
            (int? octave, int len) = _fretParser.Parse(span.Slice(i));
            if (!octave.HasValue)
                throw new InvalidOperationException();
            i += len;
            octaveOpt = octave;
        }

        if (span.Length > i && !char.IsWhiteSpace(span[i]))
        {
            return ParseNoteOrFretResult.FromError();
        }

        int length = i - start;
        return ParseNoteOrFretResult.FromNote(noteEnum, octaveOpt,start, length);
    }
}