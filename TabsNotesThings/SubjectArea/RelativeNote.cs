namespace TabsNotesThings.ViewModels;

public record RelativeNote(string Key, int Octave, decimal Hz, int HalfToneIdx, Note RootNote)
    : Note(Key, Octave, Hz, HalfToneIdx)
{
    public int HalfTonesDiffFromRoot { get; } = HalfToneIdx - RootNote.HalfToneIdx;

    public static RelativeNote FromRootNote(Note rootNote, int halfTonesDiff)
    {
        var halfToneIdx = rootNote.HalfToneIdx + halfTonesDiff;
        var note = AllNotes[halfToneIdx];

        return new RelativeNote(note.Key, note.Octave, note.Hz, note.HalfToneIdx, rootNote);
    }

    public RelativeNote() : this(default, default, default, default, default)
    {
        
    }
}