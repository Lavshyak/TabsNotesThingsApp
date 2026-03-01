using Microsoft.Extensions.DependencyInjection;
using TabsNotesThings.ViewModels;
using TabsNotesThings.Views;

namespace TabsNotesThings;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<ImportFromTextViewModel>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<TabViewModel>();
        
        
        collection.AddSingleton<MainWindow>();
    }
}