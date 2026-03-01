using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

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