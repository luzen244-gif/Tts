using LzKeyMap.Views;

namespace LzKeyMap;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("pickapp", typeof(AppPickerPage));
        Routing.RegisterRoute("hudmanager", typeof(HudManagerPage));
        Routing.RegisterRoute("viewsettings", typeof(ViewSettingsPage));
        Routing.RegisterRoute("devices", typeof(DevicesPage));
        Routing.RegisterRoute("permissions", typeof(PermissionsPage));
    }
}
