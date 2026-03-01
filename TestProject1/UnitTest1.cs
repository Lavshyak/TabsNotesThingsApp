using TabsNotesThings.ViewModels;

namespace TestProject1;

public class UnitTest1
{
    public record TestData(string Input, TabParser.ParseNoteResult Expected);

    static UnitTest1()
    {
        TabParserParseNoteWorksTestData =
        ((TestData[])[
            new TestData(" A 2", new TabParser.ParseNoteResult(
                NoteEnum: NoteEnum.A, Octave: null, StartIdx: 1, Length: 1, IsError: false)),
            new TestData("A# G", new TabParser.ParseNoteResult(
                NoteEnum: NoteEnum.ASharp, Octave: null, StartIdx: 0, Length: 2, IsError: false)),
            new TestData(" F#4 6", new TabParser.ParseNoteResult(
                NoteEnum: NoteEnum.A, Octave: 4, StartIdx: 1, Length: 3, IsError: false)),
            new TestData(" F#4 6", new TabParser.ParseNoteResult(
                NoteEnum: NoteEnum.A, Octave: 4, StartIdx: 1, Length: 3, IsError: false)),
        ]).Select(td => object(td));
    }

    public static IEnumerable<object[]> TabParserParseNoteWorksTestData;


    [Theory]
    [MemberData(nameof(TabParserParseNoteWorksTestData))]
    public void TabParserParseNoteWorks(string input, TabParser.ParseNoteResult expected)
    {
        var tabParser = TabParser.Instance;
    }
}