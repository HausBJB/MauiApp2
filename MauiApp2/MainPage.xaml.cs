using Shiny;
using Shiny.BluetoothLE;
using System.Reactive.Linq;

namespace MauiApp2;

public partial class MainPage : ContentPage
{
    private const int WaitTime = 500;
    private readonly IBleManager bleManager;

    public IDisposable ScannerDisposable { get; private set; }
    public IPeripheral CurrentDevice { get; private set; }
    public IDisposable NotifyCharacteristicDisposable { get; private set; }
    public bool Lock { get; private set; }

    public MainPage(IBleManager  bleManager)
    {
        InitializeComponent();
        this.bleManager = bleManager;
    }

    private async void Search(object sender, EventArgs e)
    {
        var access = await bleManager.RequestAccess();
        if (access == AccessState.Available)
        {
            ScannerDisposable = bleManager.Scan().Subscribe(scanResult =>
            {
                if(scanResult != null && scanResult.Peripheral.Name != null && scanResult.Peripheral.Name.StartsWith("8c"))
                {
                    ScannerDisposable?.Dispose();
                    CurrentDevice = scanResult.Peripheral;
                    deviceText.Text = scanResult.Peripheral.Name;

                }

            });

            _ =Task.Run(async() =>
            {
                await Task.Delay(5000);
                ScannerDisposable?.Dispose();

            });
        }
    }

    private async void StartTest(object sender, EventArgs e)
    {
        try
        {
            if(Lock)
                return;

            Lock = true;

            Console.WriteLine(".........................................................................Connect 1");
            await CurrentDevice.ConnectAsync(timeout: TimeSpan.FromSeconds(5));
            await CurrentDevice.TryRequestMtuAsync(255);
            await SetCharacteristics();            
            Console.WriteLine(".........................................................................Connect OK");
            await Task.Delay(WaitTime);
            CurrentDevice.CancelConnection();
            await Task.Delay(WaitTime);
            Console.WriteLine(".........................................................................Connect 2");
            await CurrentDevice.ConnectAsync(timeout: TimeSpan.FromSeconds(5));
            await CurrentDevice.TryRequestMtuAsync(255);
            await SetCharacteristics();
            Console.WriteLine(".........................................................................Connect 2 OK");
            await Task.Delay(WaitTime);
            CurrentDevice.CancelConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());   
        }finally { Lock = false; }

    }

    private async Task SetCharacteristics()
    {

        var characteristics = await CurrentDevice.GetAllCharacteristicsAsync();
        if (characteristics != null)
        {
            var characteristic = characteristics.FirstOrDefault(c => c.CanNotify());
            if (characteristic != null)
            {
                NotifyCharacteristicDisposable?.Dispose();
                NotifyCharacteristicDisposable = CurrentDevice.NotifyCharacteristic(characteristic).Subscribe(d =>
                {

                });

                await CurrentDevice.WaitForCharacteristicSubscriptionAsync(characteristic.Service.Uuid,characteristic.Uuid);
            }
        }




    }
}

