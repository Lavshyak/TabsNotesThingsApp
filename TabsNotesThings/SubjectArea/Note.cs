using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

[ObservableObject]
public partial class NoteText
{
    public NoteText(string text)
    {
        Text = text;
    }

    [ObservableProperty]
    private string _text;
}

public partial class RelativeNoteText : NoteText
{
    public RelativeNoteText(string text) : base(text)
    {
    }
}

[ObservableObject]
public partial class NoteRowText
{
    public NoteRowText(NoteText rootNoteText, IReadOnlyList<RelativeNoteText> noteTexts)
    {
        RootNoteText = rootNoteText;
        NoteTexts = noteTexts;
    }

    [ObservableProperty]
    private NoteText _rootNoteText;

    [ObservableProperty]
    private IReadOnlyList<RelativeNoteText> _noteTexts;
}

public sealed class NotePitches
{
    public static NotePitches Instance { get; } = new NotePitches();

    public decimal A4PitchHz { get; }

    public NotePitches(decimal a4PitchHz = 440m)
    {
        if (a4PitchHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(a4PitchHz));

        A4PitchHz = a4PitchHz;
    }

    /// <summary>
    /// Возвращает частоту ноты относительно A4.
    /// halfToneOffset:
    ///  0  -> A4
    ///  1  -> A#4
    /// -1  -> G#4
    /// 12  -> A5
    /// </summary>
    public decimal GetFrequency(int halfToneOffset)
    {
        double factor = Math.Pow(2.0, halfToneOffset / 12.0);
        return A4PitchHz * (decimal)factor;
    }

    public IEnumerable<decimal> FromA4Up()
    {
        for (int i = 0;; i++)
            yield return GetFrequency(i);
    }

    public IEnumerable<decimal> FromA4Down()
    {
        for (int i = 0;; i--)
            yield return GetFrequency(i);
    }
}

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

public class NotesCollectionContainer
{
    public static NotesCollectionContainer InstanceFromMinus2To10 =
        new NotesCollectionContainer(new NoteWithOctave(NoteEnum.C, -2), new NoteWithOctave(NoteEnum.B, 10));

    public record NoteInfo(NoteWithOctave NoteWithOctave, int IndexRelativeToA4, decimal Hz);

    public IReadOnlyDictionary<int, NoteInfo> IndexRelativeToA4ToNoteInfoDictionary { get; }

    public NotesCollectionContainer(NoteWithOctave from, NoteWithOctave to)
    {
        var fromIdx = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(from.NoteEnum, from.Octave);
        var toIdx = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(to.NoteEnum, to.Octave);
        IndexRelativeToA4ToNoteInfoDictionary = Enumerable.Range(fromIdx, toIdx - fromIdx+1).Select(idx =>
        {
            var (note, octave) = NoteStrs.Instance.ToNoteAndOctave(idx);
            var hz = NotePitches.Instance.GetFrequency(idx);
            var noteInfo = new NoteInfo(new NoteWithOctave(note, octave), idx, hz);
            var kvp = KeyValuePair.Create(idx, noteInfo);
            return kvp;
        }).ToFrozenDictionary();
    }

    public NoteInfo FindNearest(int anchorIdxRelativeToA4, NoteEnum noteEnum)
    {
        NoteInfo right;
        for (int i = anchorIdxRelativeToA4;; i++)
        {
            if (IndexRelativeToA4ToNoteInfoDictionary[i].NoteWithOctave.NoteEnum == noteEnum)
            {
                right = IndexRelativeToA4ToNoteInfoDictionary[i];
                break;
            }
        }

        NoteInfo left;
        for (int i = anchorIdxRelativeToA4 - 1;; i--)
        {
            if (IndexRelativeToA4ToNoteInfoDictionary[i].NoteWithOctave.NoteEnum == noteEnum)
            {
                left = IndexRelativeToA4ToNoteInfoDictionary[i];
                break;
            }
        }

        if (anchorIdxRelativeToA4 - left.IndexRelativeToA4 < right.IndexRelativeToA4 - anchorIdxRelativeToA4)
        {
            return left;
        }
        else
        {
            return right;
        }
    }
}

public record NoteNormal(NoteEnum Note, int Octave);

public record Note(string Key, int Octave, decimal Hz, int HalfToneIdx)
{
    public string KeyOctaveHzString { get; } = $"{Key}{Octave}:{Hz}";

    public string KeyOctaveString { get; } = $"{Key}{Octave}";

    private static readonly decimal Multiplier = (decimal)Math.Pow(2, (double)(1 / (decimal)12));

    private static readonly IReadOnlyList<string> OctaveNotes = [.."C C# D D# E F F# G G# A A# B".Split(' ')];

    public static IReadOnlyList<Note> AllNotes { get; } = [..FromC0()];

    private static IEnumerable<Note> FromC0()
    {
        decimal nextHz = (decimal)16.35;

        int halfToneIdx = 0;

        for (int iOctave = 0; iOctave < 9; iOctave++)
        {
            foreach (var octaveNote in OctaveNotes)
            {
                yield return new Note(octaveNote, iOctave, nextHz, halfToneIdx);
                nextHz *= Multiplier;
                halfToneIdx += 1;
            }
        }
    }

    public Note() : this(default, default, default, default)
    {
    }
}