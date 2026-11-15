using LzKeyMap.Core.Abstractions;
using LzKeyMap.Core.Devices;
using LzKeyMap.Core.Execution;
using LzKeyMap.Core.Permissions;
using LzKeyMap.Core.Storage;
using LzKeyMap.Platforms.AndroidImpl;
using LzKeyMap.ViewModels;
using LzKeyMap.Views;

namespace LzKeyMap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var s = builder.Services;

        // armazenamento (HUDs só pelo ProfileStore)
        s.AddSingleton<ProfileStore>();
        s.AddSingleton<ChosenAppsStore>();

        // métodos de execução
        s.AddSingleton<AccessibilityInputExecutor>();
        s.AddSingleton<RootInputExecutor>();
        s.AddSingleton<ShizukuInputExecutor>();
        s.AddSingleton<AdbInputExecutor>();
        s.AddSingleton<ExecutionManager>(sp => new ExecutionManager(new IInputExecutor[]
        {
            sp.GetRequiredService<AccessibilityInputExecutor>(),
            sp.GetRequiredService<RootInputExecutor>(),
            sp.GetRequiredService<ShizukuInputExecutor>(),
            sp.GetRequiredService<AdbInputExecutor>()
        }));
        s.AddSingleton<IExecutionManager>(sp => sp.GetRequiredService<ExecutionManager>());

        s.AddSingleton<PermissionManager>();
        s.AddSingleton<DeviceManager>();

        // plataforma Android
        s.AddSingleton<IGameCatalog, AndroidGameCatalog>();
        s.AddSingleton<IFileTransfer, AndroidFileTransfer>();
        s.AddSingleton<IHudEditorLauncher, AndroidHudEditorLauncher>();
        s.AddSingleton<ISystemSettingsLauncher, AndroidSystemSettingsLauncher>();
        s.AddSingleton<IEngineController, EngineController>();

        // telas
        s.AddTransient<HomeViewModel>();        s.AddTransient<HomePage>();
        s.AddTransient<AppPickerViewModel>();   s.AddTransient<AppPickerPage>();
        s.AddTransient<HudManagerViewModel>();  s.AddTransient<HudManagerPage>();
        s.AddTransient<ViewSettingsViewModel>(); s.AddTransient<ViewSettingsPage>();
        s.AddTransient<DevicesViewModel>();     s.AddTransient<DevicesPage>();
        s.AddTransient<PermissionsViewModel>(); s.AddTransient<PermissionsPage>();

        return builder.Build();
    }
}
