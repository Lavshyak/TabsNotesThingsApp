using System;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public sealed class NoteStrs
{
    public static NoteStrs Instance { get; } = new NoteStrs();
    
    public int ToHalfToneIdxRelativeToA4(NoteEnum note, int octave)
    {
        int octaveDiff = octave - 4;
        var offsetInsideOctave = NoteEnumArr.Instance.NoteEnumToOffsetFromADict[note];
        var result = octaveDiff * 12 + offsetInsideOctave;
        return result;
    }

    public int ToHalfToneIdxRelativeToA4(NoteWithOctave noteWithOctave)
    {
        return ToHalfToneIdxRelativeToA4(noteWithOctave.NoteEnum, noteWithOctave.Octave);
    }

    public (NoteEnum note, int octave) ToNoteAndOctave(int halfToneIdxRelativeToA4)
    {
        int octaveOffset = (int)Math.Floor(halfToneIdxRelativeToA4 / 12.0);
        int octave = 4 + octaveOffset;
        int noteOffset = halfToneIdxRelativeToA4 - octaveOffset * 12;
        if (noteOffset > 2)
        {
            noteOffset -= 12;
            octave++;
        }

        var noteEnum = NoteEnumArr.Instance.NoteOffsetToNoteEnumDict[noteOffset];
        return (noteEnum, octave);
    }

    public NoteWithOctave ToNoteWithOctave(int halfToneIdxRelativeToA4)
    {
        var (note, octave) = ToNoteAndOctave(halfToneIdxRelativeToA4);
        return new NoteWithOctave(note, octave);
    }

    public string ToStringNoteOctaveFromHalfToneIdx(int halfToneIdxRelativeToA4)
    {
        var (note, octave) = ToNoteAndOctave(halfToneIdxRelativeToA4);
        var str = ToStringNoteOctave(note, octave);
        return str;
    }

    public string ToStringNote(NoteEnum note)
    {
        return $"{NotesEnumToStrings.Instance.NoteEnumToNoteNameDict[note]}";
    }

    public string ToStringNoteOctave(NoteEnum note, int octave)
    {
        return $"{NotesEnumToStrings.Instance.NoteEnumToNoteNameDict[note]}{octave}";
    }
}