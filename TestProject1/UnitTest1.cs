using TabsNotesThings.ViewModels;
using TabsNotesThings.ViewModels.ReadOnlyModels;
using static TabsNotesThings.ViewModels.Parsers.NoteOrFretParser;

namespace TestProject1;

public class UnitTest1
{
    private static object[] ToObjArr1(int testId, string input, int startIdx,
        ParseNoteOrFretResult parseNoteOrFretResult)
    {
        return [testId, input, startIdx, parseNoteOrFretResult];
    }

    public static IEnumerable<object[]> TabParserParseNoteFromSpanWorksTestData =>
    [
        ToObjArr1(0, "A", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.A, octave: null, startIdx: 0, length: 1)),
        ToObjArr1(1, " A", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.A, octave: null, startIdx: 1, length: 1)),
        ToObjArr1(2, " A 2", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.A, octave: null, startIdx: 1, length: 1)),
        ToObjArr1(3, "A# G", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.ASharp, octave: null, startIdx: 0, length: 2)),
        ToObjArr1(4, " F#4 6", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.FSharp, octave: 4, startIdx: 1, length: 3)),
        ToObjArr1(5, " F#5 B", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.FSharp, octave: 5, startIdx: 1, length: 3)),
        ToObjArr1(6, " f#5 B", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.FSharp, octave: 5, startIdx: 1, length: 3)),
        ToObjArr1(7, " 4 ", 0,
            ParseNoteOrFretResult.FromFret(fret: 4, startIdx: 1, length: 1)),
        ToObjArr1(8, " 5A ", 0,
            ParseNoteOrFretResult.FromError()),
        ToObjArr1(9, " 6 A ", 0,
            ParseNoteOrFretResult.FromFret(fret: 6, startIdx: 1, length: 1)),
        ToObjArr1(10, " ", 0,
            ParseNoteOrFretResult.FromEmpty()),
        ToObjArr1(11, "X", 0,
            ParseNoteOrFretResult.FromError()),
        ToObjArr1(12, "", 0,
            ParseNoteOrFretResult.FromEmpty()),
        ToObjArr1(13, " l A", 0,
            ParseNoteOrFretResult.FromError()),
        ToObjArr1(14, "C F#5 B", 1,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.FSharp, octave: 5, startIdx: 1, length: 3)),
        ToObjArr1(15, "C F#5 B", 2,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.FSharp, octave: 5, startIdx: 0, length: 3)),
        ToObjArr1(16, "C F#5 B", 0,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.C, octave: null, startIdx: 0, length: 1)),
        ToObjArr1(17, "C F#5 B", 3,
            ParseNoteOrFretResult.FromError()),
        ToObjArr1(18, "C F#5 B", 5,
            ParseNoteOrFretResult.FromNote(noteEnum: NoteEnum.B, octave: null, startIdx: 1, length: 1)),
    ];

    /*[Fact]
    public void Hui()
    {
        var kvm = new KeyboardViewModel();
        
        kvm.HandleInputCommand.Execute(new TabViewModel.Tab2([
                    new NoteMayBeWithOctave(NoteEnum.FSharp, 2), null, new NoteMayBeWithOctave(NoteEnum.B, null)
                ],
                [
                    [18, null, null],
                    [5, null, 5],
                    [null, null, 13]
                ]
            )
        );
    }*/
    [Fact]
    public void Hui1()
    {
        var obj = new NotesCollectionContainer(new NoteWithOctave(NoteEnum.C, 4), new NoteWithOctave(NoteEnum.B, 4));
    }

    [Theory]
    [MemberData(nameof(TabParserParseNoteFromSpanWorksTestData))]
    public void TabParserParseNoteFromSpanWorks(
        int testId,
        string input,
        int startIdx,
        ParseNoteOrFretResult expected)
    {
        var parser = Instance;

        var result = parser.ParseNextNoteOrFret(input.AsSpan(startIdx));


        Assert.Equal(expected.NoteEnum, result.NoteEnum);
        Assert.Equal(expected.Octave, result.Octave);
        Assert.Equal(expected.Fret, result.Fret);
        Assert.Equal(expected.StartIdx, result.StartIdx);
        Assert.Equal(expected.Length, result.Length);
        Assert.Equal(expected.IsError, result.IsError);
        Assert.Equal(expected.IsEmpty, result.IsEmpty);
    }
}