using System.Text.Json;
using TabsNotesThings.ViewModels;
using TabsNotesThings.ViewModels.ReadOnlyModels;
using Xunit.Abstractions;

namespace TestProject1;

public class HtIdxToNoteOctaveTest
{
    private readonly ITestOutputHelper _log;

    public HtIdxToNoteOctaveTest(ITestOutputHelper log)
    {
        _log = log;
    }

    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(-20)]
    [Theory]
    public void ToNoteAndOctave(int htIdx)
    {
        var result = NoteStrs.Instance.ToNoteAndOctave(htIdx);
        _log.WriteLine(JsonSerializer.Serialize(new
        {
            htIdx,
            result = new{ result.note, result.octave }
        }));
    }

    private static object[] ToHui3Data(int htIdx, NoteWithOctave expected)
    {
        return [htIdx, expected];
    }
    public static IReadOnlyList<object[]> Hui3Data = [
        ToHui3Data(0, new NoteWithOctave(NoteEnum.A, 4)),
        ToHui3Data(-1, new NoteWithOctave(NoteEnum.GSharp, 4)),
        ToHui3Data(1, new NoteWithOctave(NoteEnum.ASharp, 4)),
        ToHui3Data(2, new NoteWithOctave(NoteEnum.B, 4)),
        ToHui3Data(-9,new NoteWithOctave(NoteEnum.C, 4)),
        ToHui3Data(-8,new NoteWithOctave(NoteEnum.CSharp, 4)),
        ToHui3Data(3,new NoteWithOctave(NoteEnum.C, 5)),
        ToHui3Data(12,new NoteWithOctave(NoteEnum.A, 5)),
        ToHui3Data(-12,new NoteWithOctave(NoteEnum.A, 3)),
        ToHui3Data(-11,new NoteWithOctave(NoteEnum.ASharp, 3)),
        ToHui3Data(-16,new NoteWithOctave(NoteEnum.F, 3)),
        ToHui3Data(-48,new NoteWithOctave(NoteEnum.A, 0)),
        ToHui3Data(-60,new NoteWithOctave(NoteEnum.A, -1)),
        ToHui3Data(-72,new NoteWithOctave(NoteEnum.A, -2)),
        ToHui3Data(-73,new NoteWithOctave(NoteEnum.GSharp, -2)),
    ];
    
    [MemberData(nameof(Hui3Data))]
    [Theory]
    public void Hui3(int htIdx, NoteWithOctave expected)
    {
        var instance = NoteStrs.Instance;
        var result = instance.ToNoteAndOctave(htIdx);
        Assert.Equal(expected.NoteEnum, result.note);
        Assert.Equal(expected.Octave, result.octave);
    }
    
    [MemberData(nameof(Hui3Data))]
    [Theory]
    public void Hui4(int htIdxExpected, NoteWithOctave input)
    {
        var instance = NoteStrs.Instance;
        var result = instance.ToHalfToneIdxRelativeToA4(input);
        Assert.Equal(htIdxExpected, result);
    }
}