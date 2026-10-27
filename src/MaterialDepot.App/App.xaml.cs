using System.Globalization;
using System.IO;
using System.Windows;
using MaterialDepot.App.Services;
using MaterialDepot.App.ViewModels;
using MaterialDepot.Core.Downloads;
using MaterialDepot.Core.Imaging;
using MaterialDepot.Core.Library;
using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Setup;
using MaterialDepot.Core.Stride;
using Microsoft.Extensions.DependencyInjection;

namespace MaterialDepot.App;

public partial class App : Application
{
    private ServiceProvider? services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        services = ConfigureServices().BuildServiceProvider();
        services.GetRequiredService<AppLanguage>().Apply(services.GetRequiredService<DepotSession>().Settings.Language);

        var viewModel = services.GetRequiredService<MainViewModel>();
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
        await viewModel.StartAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceCollection ConfigureServices()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaterialDepot");
        var localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MaterialDepot");
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var services = new ServiceCollection();
        services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore(Path.Combine(appData, "settings.json"), () => DepotSettings.CreateDefault(userProfile)));
        services.AddSingleton<IImageConverter, MagickImageConverter>();
        services.AddSingleton<IThumbnailCache>(provider => new ThumbnailCache(provider.GetRequiredService<IImageConverter>(), Path.Combine(localAppData, "Thumbnails")));
        services.AddSingleton<ITextureLibraryScanner, TextureLibraryScanner>();
        services.AddSingleton<IDownloadScanner, DownloadScanner>();
        services.AddSingleton<IFileRemover, RecycleBinFileRemover>();
        services.AddSingleton<IDownloadArchiver, DownloadArchiver>();
        services.AddSingleton<IStrideProjectInventory, StrideProjectInventory>();
        services.AddSingleton<IStrideMaterialImporter, StrideMaterialImporter>();

        services.AddSingleton(new AppLanguage(CultureInfo.CurrentUICulture));
        services.AddSingleton<SetupGuide>();
        services.AddSingleton<DepotSession>();
        services.AddSingleton<ThumbnailLoader>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddSingleton<LibraryViewModel>();
        services.AddSingleton<InboxViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();
        return services;
    }
}
