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
}