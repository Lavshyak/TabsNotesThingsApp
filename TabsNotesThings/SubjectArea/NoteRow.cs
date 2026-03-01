using System.Collections.Generic;

namespace TabsNotesThings.ViewModels;

public record NoteRow(Note RootNote, IReadOnlyList<RelativeNote?> Notes)
{
    public NoteRow() : this(null, null)
    {
        
    }
};