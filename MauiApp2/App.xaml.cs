using Shiny.BluetoothLE;

namespace MauiApp2;

public partial class App : Application
{
    public App(IBleManager bleManager)
    {
        InitializeComponent();

        MainPage = new MainPage(bleManager);
    }
}
