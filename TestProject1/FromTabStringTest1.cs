using TabsNotesThings.ViewModels;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TestProject1;

public class FromTabStringTest1
{
    private static object?[] ToObjArr1(int testId, string input, List<NoteMayBeWithOctave?>? rootsOut,
        List<int?[]>? columnsOut, bool isError)
    {
        return [testId.ToString(), input, rootsOut, columnsOut, isError];
    }
    
    private static object?[] ToObjArr1(string testId, string input, List<NoteMayBeWithOctave?>? rootsOut,
        List<int?[]>? columnsOut, bool isError)
    {
        return [testId, input, rootsOut, columnsOut, isError];
    }

    public static IEnumerable<object?[]> TestData =>
    [
        ToObjArr1("00",
            "",
            [null],
            [],
            false
        ),
        ToObjArr1("01",
            " ",
            [null],
            [],
            false
        ),
        ToObjArr1("02",
            """

            
            """,
            [null, null],
            [],
            false
        ),
        ToObjArr1(2,
            ".",
            [],
            [],
            true
        ),
        ToObjArr1(3,
            "A",
            [new(NoteEnum: NoteEnum.A, Octave: null)],
            [],
            false
        ),
        ToObjArr1(4,
            "X",
            null,
            null,
            true
        ),
        ToObjArr1(5,
            "B 3 0 12",
            [new(NoteEnum: NoteEnum.B, Octave: null)],
            [[3], [0], [12]],
            false
        ),
        ToObjArr1(6,
            """
            B 3 0
            C 5 6
            """,
            [new(NoteEnum: NoteEnum.B, Octave: null), new(NoteEnum: NoteEnum.C, null)],
            [[3, 5], [0, 6]],
            false
        ),
        ToObjArr1(7,
            """
            B  3 0
            C 5 6
            """,
            [new(NoteEnum: NoteEnum.B, Octave: null), new(NoteEnum: NoteEnum.C, null)],
            [[null,5], [3, null], [null,6], [0, null]],
            false
        ),
        ToObjArr1(8,
            """
            B   3   0 2 11 

            C#2 5 6 5   4  
            """,
            [new(NoteEnum: NoteEnum.B, Octave: null), null, new(NoteEnum: NoteEnum.CSharp, 2)],
            [[3, null, 5], [null, null, 6], [0, null, 5], [2, null, null], [11, null, 4]],
            false
        ),
        ToObjArr1(9,
            """

            B   3   0 2 11 
                1
            C#2 5 6 5   4  
            """,
            null,
            null,
            true
        ),
        ToObjArr1(10,
            """
            B   3   0 2 11  1 
            C#2 5 6 5   4  12
            """,
            null,
            null,
            true
        ),
        ToObjArr1(11,
            // смещение от левого края
            """
             B 3 0
              C 5 6
            """,
            [new NoteMayBeWithOctave(NoteEnum.B, null), new NoteMayBeWithOctave(NoteEnum.C, null)],
            [[3,null], [null,5], [0, null], [null,6]],
            false
        )
    ];

    [Theory]
    [InlineData("9")]
    public void Test(string testId)
    {
        var data = TestData.First(x=>(string)x[0] == testId);
        TabParserFromTabStringWorks((string)data[0], (string)data[1], (List<NoteMayBeWithOctave?>?)data[2], (List<int?[]>?)data[3], (bool) data[4]);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void TabParserFromTabStringWorks(
        string testId,
        string input,
        List<NoteMayBeWithOctave?>? rootsOut,
        List<int?[]>? columnsOut,
        bool isError)
    {
        /*if (testId != "5")
        {
            return;
        }*/
        
        var parser = TabParser.Instance;

        var result = parser.FromTabString(input);

        Assert.Equal(isError, result.IsError);
        if (isError)
        {
            Assert.Null(result.RootNotes);
            Assert.Null(result.Columns);
        }
        else
        {
            if (rootsOut == null || columnsOut == null)
            {
                throw new InvalidOperationException("ошибка в тесте вроде бы");
            }

            Assert.NotNull(result.RootNotes);
            Assert.Equal(rootsOut.Count, result.RootNotes.Count);
            Assert.All(result.RootNotes, (n, i) =>
            {
                var expected = rootsOut[i];
                if (expected == null)
                {
                    Assert.Null(n);
                }
                else
                {
                    Assert.NotNull(n);
                    Assert.Equal(expected.NoteEnum, n.NoteEnum);
                    Assert.Equal(expected.Octave, n.Octave);
                }
            });

            Assert.NotNull(result.Columns);
            Assert.Equal(columnsOut.Count, result.Columns.Count);
            Assert.All(result.Columns, (c, i) =>
                {
                    var expectedColumn = columnsOut[i];

                    Assert.NotNull(c);
                    Assert.Equal(c.Length, expectedColumn.Length);
                    Assert.All(c, (v, iV) =>
                    {
                        var expectedV = expectedColumn[iV];
                        Assert.Equal(expectedV, v.FretNumber);
                    });
                }
            );
        }
    }
}