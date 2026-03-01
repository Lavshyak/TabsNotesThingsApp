using TabsNotesThings.ViewModels;

namespace TestProject1;

public class UnitTest1
{
    public record TestData(string Input, TabParser.ParseNoteResult Expected);

    private static object[] ToObjArr(string input, TabParser.ParseNoteResult parseNoteResult)
    {
        return [input, parseNoteResult];
    }

    public static IEnumerable<object[]> TabParserParseNoteWorksTestData =>
    [
        ToObjArr("A", new(NoteEnum: NoteEnum.A, Octave: null, StartIdx: 0, Length: 1, IsError: false)),
        ToObjArr(" A 2", new(NoteEnum: NoteEnum.A, Octave: null, StartIdx: 1, Length: 1, IsError: false)),
        ToObjArr("A# G", new(NoteEnum: NoteEnum.ASharp, Octave: null, StartIdx: 0, Length: 2, IsError: false)),
        ToObjArr(" F#4 6", new(NoteEnum: NoteEnum.FSharp, Octave: 4, StartIdx: 1, Length: 3, IsError: false)),
        ToObjArr(" F#5 B", new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 1, Length: 3, IsError: false)),
        ToObjArr(" f#5 B", new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 1, Length: 3, IsError: false)),
        ToObjArr(" 4 ", new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr(" ", new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr("", new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true))
    ];

    [Theory]
    [MemberData(nameof(TabParserParseNoteWorksTestData))]
    public void TabParserParseNoteWorks(
        string input,
        TabParser.ParseNoteResult expected)
    {
        var parser = TabParser.Instance;

        var result = parser.ParseNote(input);

        Assert.False(result.IsError);
        Assert.Equal(expected.NoteEnum, result.NoteEnum);
        Assert.Equal(expected.Octave, result.Octave);
        Assert.Equal(expected.StartIdx, result.StartIdx);
        Assert.Equal(expected.Length, result.Length);
    }
}