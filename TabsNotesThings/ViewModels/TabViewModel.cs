using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public partial class TabViewModel : ViewModelBase
{
    private readonly KeyboardViewModel _keyboardViewModel;

    public TabViewModel()
    {
        _keyboardViewModel = KeyboardViewModel.Instance;
    }

    public TabViewModel(KeyboardViewModel keyboardViewModel)
    {
        _keyboardViewModel = keyboardViewModel;
    }

    public static TabViewModel Instance { get; } = new TabViewModel();

    [ObservableProperty]
    private string _tablatureInputText = Tab;

    private const string Tab =
        """
        C#     3 3 2 3          
        G#  5                   
        E2                    
        B      0 0   0     
        F#6 2      4       
        B   2  5 5 4 5    

        C#    3 3 2 3          
        G#  5                   
        E2                   
        B      0 0   0     
        F#6 2      4       
        B   2  5 5 4 5    
        """;

    private string _previousText = Tab;

    partial void OnTablatureInputTextChanged(string value)
    {
        HandleSymbol(value, _previousText, SpecialSymbols.Instance.Pause);
        HandleSymbol(value, _previousText, SpecialSymbols.Instance.SquareEnd);
        HandleSymbol(value, _previousText, SpecialSymbols.Instance.Ligature);
        _previousText = value;
    }

    private bool _isHandleSymbolEnabled = true;
    void HandleSymbol(string currentText, string previousText, char symbol)
    {
        if (currentText == null || previousText == null)
            return;

        if (_isHandleSymbolEnabled == false)
        {
            return;
        }

        bool posted = false;
        try
        {
            _isHandleSymbolEnabled = false;

            var (diffIndexNullable, diffTypeEnum) = GetFirstDiffIndex(currentText, previousText);

            if (diffIndexNullable == null || diffTypeEnum == DiffTypeEnum.None)
            {
                return;
            }

            var diffIndex = diffIndexNullable.Value;

            if (diffTypeEnum == DiffTypeEnum.Added)
            {
                if (currentText[diffIndex] != symbol)
                    return;

                // добавить \' везде
                var lines = currentText.Split((string[])["\r\n", "\n"], StringSplitOptions.None);
                int lineIndex = currentText.Substring(0, diffIndex).Count(c => c == '\n');
                int column = diffIndex - currentText.LastIndexOf('\n', diffIndex >= 1 ? diffIndex - 1 : 0) - 1;

                int startPartIdx = 0;
                int partLen = 0;
                bool isVisited = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == lineIndex)
                    {
                        isVisited = true;
                    }

                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        if (isVisited)
                        {
                            break;
                        }
                        else
                        {
                            startPartIdx = i + 1;
                            partLen = 0;
                        }
                    }
                    else
                    {
                        partLen++;
                    }
                }

                if (isVisited == false || partLen == 0 || startPartIdx >= lines.Length)
                {
                    return;
                }

                var sb = new StringBuilder();

                if (startPartIdx > 0)
                {
                    sb.Append(string.Join("\n", lines.Take(startPartIdx)));
                    sb.Append("\n"); // строка с idx = startPartIdx точно есть
                }

                for (int i = startPartIdx; i < startPartIdx + partLen; i++)
                {
                    var line = lines[i];

                    if (i != lineIndex)
                    {
                        if (column < line.Length && line[column] != symbol)
                            line = line.Insert(column, symbol.ToString());
                        else
                            line = line.PadRight(column, ' ') + symbol;
                    }

                    sb.Append(line);

                    if (i < lines.Length - 1)
                        sb.Append('\n');
                }


                sb.Append(string.Join("\n", lines.Skip(startPartIdx + partLen)));


                var result = sb.ToString();
                Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            TablatureInputText = result;
                        }
                        finally
                        {
                            _isHandleSymbolEnabled = true;
                        }
                    }
                );
                posted = true;
                return;
            }
            else if (diffTypeEnum == DiffTypeEnum.Replaced)
            {
                return;
            }
            else if (diffTypeEnum == DiffTypeEnum.Removed)
            {
                if (previousText[diffIndex] != '\'')
                    return;

                // удалить \' везде

                var lines = currentText.Split('\n');
                int lineIndex = currentText.Substring(0, diffIndex).Count(c => c == '\n');
                int column = diffIndex - currentText.LastIndexOf('\n', diffIndex >= 1 ? diffIndex - 1 : 0) - 1;

                var sb = new StringBuilder();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (i != lineIndex)
                    {
                        if (column < line.Length && line[column] == symbol)
                            line = line.Remove(column, 1);
                    }

                    sb.Append(line);

                    if (i < lines.Length - 1)
                        sb.Append('\n');
                }

                var result = sb.ToString();
                Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            TablatureInputText = result;
                        }
                        finally
                        {
                            _isHandleSymbolEnabled = true;
                        }
                    }
                );
                posted = true;
                return;
            }
        }
        finally
        {
            if (!posted)
            {
                _isHandleSymbolEnabled = true;
            }
        }
    }

    enum DiffTypeEnum
    {
        Added,
        Replaced,
        Removed,
        None
    }

    private (int? idx, DiffTypeEnum diffTypeEnum) GetFirstDiffIndex(string current, string previous)
    {
        DiffTypeEnum diffType;
        int? idx = null;
        if (current.Length > previous.Length)
        {
            diffType = DiffTypeEnum.Added;
            int len = Math.Min(current.Length, previous.Length);
            for (int i = 0; i < len; i++)
            {
                if (current[i] != previous[i])
                {
                    idx = i;
                    break;
                }
            }


            if (idx.HasValue == false)
            {
                idx = len;
            }
        }
        else if (current.Length == previous.Length)
        {
            diffType = DiffTypeEnum.Replaced;
            int len = Math.Min(current.Length, previous.Length);
            for (int i = 0; i < len; i++)
            {
                if (current[i] != previous[i])
                {
                    idx = i;
                    break;
                }
            }

            if (idx.HasValue == false)
            {
                diffType = DiffTypeEnum.None;
            }
        }
        else
        {
            diffType = DiffTypeEnum.Removed;
            int len = Math.Min(current.Length, previous.Length);
            for (int i = 0; i < len; i++)
            {
                if (current[i] != previous[i])
                {
                    idx = i;
                    break;
                }
            }

            if (idx.HasValue == false)
            {
                idx = len;
            }
        }

        return (idx.Value, diffType);
    }


    public record Tab1(IReadOnlyList<int?> RootsFromA4Indexes, IReadOnlyList<IReadOnlyList<int?>> Columns);

    private void ToTab1(IReadOnlyList<NoteMayBeWithOctave?> rootNotes, IReadOnlyList<IReadOnlyList<int?>> columns)
    {
        var rootsFromA4Indexes = rootNotes.Select((noteMayBeWithOctave) =>
        {
            if (noteMayBeWithOctave == null)
            {
                return (int?)null;
            }

            return NoteStrs.Instance.ToHalfToneIdxRelativeToA4(noteMayBeWithOctave.NoteEnum,
                noteMayBeWithOctave.Octave ?? 4);
        }).ToImmutableArray();

        var tab1 = new Tab1(rootsFromA4Indexes, columns);
    }

    public record Tab2(IReadOnlyList<NoteMayBeWithOctave?> Roots, IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> Columns);

    public record Tab3(IReadOnlyList<NoteWithOctave?> Roots, IReadOnlyList<IReadOnlyList<TabParser.FretNumberOrSpecialSymbol>> Columns);

    [RelayCommand]
    private void ParseInput()
    {
        try
        {
            var noteRows = TabParser.Instance.FromTabString(TablatureInputText);
            if (noteRows.IsError)
            {
                ImportError();
                return;
            }

            var tab2 = new Tab2(noteRows.RootNotes, noteRows.Columns);

            _keyboardViewModel.HandleInputCommand.ExecuteExactly(tab2);

            ImportOk();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ImportError();
            return;
        }
    }

    public void ImportError()
    {
        InputBorderBrush = Avalonia.Media.Brushes.Red;
    }

    public void ImportOk()
    {
        InputBorderBrush = Avalonia.Media.Brushes.Black;
    }

    [ObservableProperty]
    private IImmutableSolidColorBrush _inputBorderBrush = Avalonia.Media.Brushes.Black;
}