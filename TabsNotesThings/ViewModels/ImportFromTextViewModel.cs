using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TabsNotesThings.ViewModels;

public partial class ImportFromTextViewModel : ViewModelBase
{
    
    
    /*[RelayCommand]
    private void Import()
    {
        ImportCalled?.Invoke(InputFieldText);
    }*/

    public event Action<string>? ImportCalled;
}