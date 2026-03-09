using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public partial class TabViewModel : ViewModelBase
{
    public static TabViewModel Instance { get; } = new TabViewModel();
    
    [ObservableProperty]
    private string _tablatureInputText = Tab;

    private const string Tab =
        """
        C#    3 3 2 3          
        G# 5                   
        E2                   
        B     0 0   0     
        F#6 2      4       
        B  2  5 5 4 5    
        """;

    public TabViewModel()
    {
    }

    private readonly KeyboardViewModel _keyboardViewModel = KeyboardViewModel.Instance;

    public record Tab1(IReadOnlyList<int?> RootsFromA4Indexes, IReadOnlyList<IReadOnlyList<int?>> Columns);
    private void ToTab1(IReadOnlyList<NoteMayBeWithOctave?> rootNotes, IReadOnlyList<IReadOnlyList<int?>> columns)
    {
        var rootsFromA4Indexes = rootNotes.Select((noteMayBeWithOctave) =>
        {
            if (noteMayBeWithOctave == null)
            {
                return (int?) null;
            }
            return NoteStrs.Instance.ToHalfToneIdxRelativeToA4(noteMayBeWithOctave.NoteEnum, noteMayBeWithOctave.Octave ?? 4);
        }).ToImmutableArray();

        var tab1 = new Tab1(rootsFromA4Indexes, columns);
    }

    public record Tab2(IReadOnlyList<NoteMayBeWithOctave?> Roots, IReadOnlyList<IReadOnlyList<int?>> Columns);

    public record Tab3(IReadOnlyList<NoteWithOctave?> Roots, IReadOnlyList<IReadOnlyList<int?>> Columns);
    
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

            _keyboardViewModel.HandleInputCommand.Execute(tab2);

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