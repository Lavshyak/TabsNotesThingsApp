using System.Collections.Generic;
using System.Linq;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public class DefaultGuitarNotes
{
    public static DefaultGuitarNotes Instance { get; } = new DefaultGuitarNotes();
    
    public IReadOnlyList<NoteWithOctave> NotesWithOctaves { get;} = [
        // 6
        new (NoteEnum.E, 4),
        new (NoteEnum.B, 3),
        new (NoteEnum.G, 3),
        new (NoteEnum.D, 3),
        new (NoteEnum.A, 2),
        new (NoteEnum.E, 2),
        //8
        //new (NoteEnum.B, 1),
        //new (NoteEnum.FSharp, 1)
    ];
    
    public NoteWithOctave GetDefaultNoteWithOctave(int stringIdx, NoteEnum noteEnum)
    {
        var anchor = NotesWithOctaves[stringIdx];
        var anchorIdxFromA4 = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(anchor.NoteEnum, anchor.Octave);
        var noteInfo = NotesCollectionContainer.InstanceFromMinus2To10.FindNearest(anchorIdxFromA4, noteEnum);
        return noteInfo.NoteWithOctave;
    }
}