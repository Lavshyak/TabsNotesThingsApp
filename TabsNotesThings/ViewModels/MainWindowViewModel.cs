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
    public MainWindowViewModel()
    {
        KeyboardViewModel = KeyboardViewModel.Instance;
        TabViewModel = TabViewModel.Instance;
    }
    
    public TabViewModel TabViewModel { get; }
    public KeyboardViewModel KeyboardViewModel { get; }
    /*[ObservableProperty]
    private string _inputFieldText = Tab;

    
    /*private const string Tab =
        """
        C# 3 3        
        B  0 0    
        B  5 5 
        """;#1#
    
    //public TabViewModel TabViewModel { get; }
    
    [ObservableProperty]
    private IReadOnlyList<NoteRowText> _noteRowTexts = [];
    
    public MainWindowViewModel() : base()
    {
        //TabViewModel = new TabViewModel();
        Import();
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
    }*/
}