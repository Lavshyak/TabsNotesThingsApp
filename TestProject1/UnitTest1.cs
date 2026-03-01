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

        Assert.Equal(expected.IsError, result.IsError);
        Assert.Equal(expected.NoteEnum, result.NoteEnum);
        Assert.Equal(expected.Octave, result.Octave);
        Assert.Equal(expected.StartIdx, result.StartIdx);
        Assert.Equal(expected.Length, result.Length);
    }
    
    private static object[] ToObjArr1(string input, int startIdx, TabParser.ParseNoteResult parseNoteResult)
    {
        return [input, startIdx, parseNoteResult];
    }
    
    public static IEnumerable<object[]> TabParserParseNoteFromSpanWorksTestData =>
    [
        ToObjArr1("A", 0, new(NoteEnum: NoteEnum.A, Octave: null, StartIdx: 0, Length: 1, IsError: false)),
        ToObjArr1(" A 2", 0, new(NoteEnum: NoteEnum.A, Octave: null, StartIdx: 1, Length: 1, IsError: false)),
        ToObjArr1("A# G", 0, new(NoteEnum: NoteEnum.ASharp, Octave: null, StartIdx: 0, Length: 2, IsError: false)),
        ToObjArr1(" F#4 6", 0, new(NoteEnum: NoteEnum.FSharp, Octave: 4, StartIdx: 1, Length: 3, IsError: false)),
        ToObjArr1(" F#5 B", 0, new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 1, Length: 3, IsError: false)),
        
        ToObjArr1(" f#5 B", 0, new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 1, Length: 3, IsError: false)),
        
        ToObjArr1(" 4 ", 0, new(NoteEnum: null, Octave: 4, StartIdx: 1, Length: null, IsError: false)),
        ToObjArr1(" 5A ", 0, new(NoteEnum: null, Octave: 5, StartIdx: 1, Length: null, IsError: false)),
        ToObjArr1(" 6 A ", 0, new(NoteEnum: null, Octave: 6, StartIdx: 1, Length: null, IsError: false)),
        
        ToObjArr1(" ", 0, new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr1("X", 0, new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr1("", 0, new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr1(" l A", 0, new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        
        ToObjArr1("C F#5 B", 1, new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 1, Length: 3, IsError: false)),
        ToObjArr1("C F#5 B", 2, new(NoteEnum: NoteEnum.FSharp, Octave: 5, StartIdx: 0, Length: 3, IsError: false)),
        ToObjArr1("C F#5 B", 0, new(NoteEnum: NoteEnum.C, Octave: null, StartIdx: 0, Length: 1, IsError: false)),
        ToObjArr1("C F#5 B", 3, new(NoteEnum: null, Octave: null, StartIdx: null, Length: null, IsError: true)),
        ToObjArr1("C F#5 B", 5, new(NoteEnum: NoteEnum.B, Octave: null, StartIdx: 1, Length: 1, IsError: false)),
    ];
    
    [Theory]
    [MemberData(nameof(TabParserParseNoteFromSpanWorksTestData))]
    public void TabParserParseNoteFromSpanWorks(
        string input,
        int startIdx,
        TabParser.ParseNoteResult expected)
    {
        var parser = TabParser.Instance;

        var result = parser.ParseNote(input.AsSpan(startIdx));

        Assert.Equal(expected.IsError, result.IsError);
        Assert.Equal(expected.NoteEnum, result.NoteEnum);
        Assert.Equal(expected.Octave, result.Octave);
        Assert.Equal(expected.StartIdx, result.StartIdx);
        Assert.Equal(expected.Length, result.Length);
    }
}