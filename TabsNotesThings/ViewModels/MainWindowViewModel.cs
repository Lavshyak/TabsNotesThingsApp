using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TabsNotesThings.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _inputFieldText = Tab;

    private const string Tab =
        """
        C#    3 3 2 3          
        G# 5                   
        E                   
        B     0 0   0     
        F# 2      4       
        B  2  5 5 4 5    
        """;
    /*private const string Tab =
        """
        C# 3 3        
        B  0 0    
        B  5 5 
        """;*/
    
    //public TabViewModel TabViewModel { get; }
    
    [ObservableProperty]
    private IReadOnlyList<NoteRowText> _noteRowTexts = [];
    
    public MainWindowViewModel() : base()
    {
        //TabViewModel = new TabViewModel();
        Import();
    }

    [ObservableProperty]
    private IImmutableSolidColorBrush _inputBorderBrush = Avalonia.Media.Brushes.Black;

    [RelayCommand]
    private void Import()
    {
        try
        {
            var noteRows = new TabParser().FromTabStringToNoteRows(InputFieldText);
            var noteRowTexts = noteRows.Select(nr =>
            {
                var rootNoteText = new NoteText(nr.RootNote.KeyOctaveString);
                var noteTexts = nr.Notes.Select(n => new RelativeNoteText(n?.HalfTonesDiffFromRoot.ToString() ?? string.Empty)).ToList();
                return new NoteRowText(rootNoteText, noteTexts);
            }).ToArray();
            NoteRowTexts = noteRowTexts;
            InputBorderBrush = Avalonia.Media.Brushes.Black;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            InputBorderBrush = Avalonia.Media.Brushes.Red;
        }
    }
    
    
    [ObservableProperty]
    private IReadOnlyList<NoteRowText> _noteRowTextsSecond = [];
    
    [RelayCommand]
    private void ImportSecond()
    {
        try
        {
            //NoteRows = new TabParser().FromTabStringToNoteRows(InputFieldText);
            InputBorderBrush = Avalonia.Media.Brushes.Black;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            InputBorderBrush = Avalonia.Media.Brushes.Red;
        }
    }
}