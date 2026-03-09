using CommunityToolkit.Mvvm.Input;

namespace TabsNotesThings;

public static class RelayCommandExtensions
{
    public static void ExecuteExactly<T>(this IRelayCommand<T> relayCommand, T parameter)
    {
        relayCommand.Execute(parameter);
    }
}