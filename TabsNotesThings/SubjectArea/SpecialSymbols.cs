using System.Collections.Generic;

namespace TabsNotesThings.ViewModels;

public class SpecialSymbols
{
    public static SpecialSymbols Instance = new SpecialSymbols();
    
    public char Pause { get; } = ',';
    public char SquareEnd { get; } = '|';
    public char Ligature { get; } = '~';
    public char Empty { get; } = '.'; //  "·"

    public IReadOnlyList<char> Symbols { get; } 
    public IReadOnlyList<char> SymbolsForStretchByColumn { get; } 

    public SpecialSymbols()
    {
        Symbols = [Pause, SquareEnd, Ligature, Empty];
        SymbolsForStretchByColumn = [SquareEnd, Pause];
    }
}