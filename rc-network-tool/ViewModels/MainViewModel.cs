using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using rc_network_tool.Models;
using rc_network_tool.Services;
using rc_network_tool.Utils;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace rc_network_tool.ViewModels;

public partial class MainViewModel(
    INetworkAdapterService networkAdapterService, 
    IMacOuiRegistryService macOuiRegistryService, 
    IAlertService alertService) 
    : ObservableObject
{
    private const int ADAPTER_SPEED_HISTORY_SIZE = 120;

    [ObservableProperty]
    public partial ObservableRangeCollection<NetworkAdapter> NetworkAdapters { get; set; } = [];

    [ObservableProperty]
    public partial ObservableRangeCollection<MacOuiRegistrant> MacOuiRegistry { get; set; } = [];

    [ObservableProperty]
    public partial NetworkAdapter? SelectedAdapter { get; set; } = null;

    [ObservableProperty]
    public partial ObservableCollection<ISeries> Series { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<long> SentSeries { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<long> ReceivedSeries { get; set; } = [];

    public Axis[] XAxes { get; set; } =
    [
        new Axis
        {
            Labels = Array.Empty<string>(),
            SeparatorsPaint = null,
        }
    ];

    public Axis[] YAxes { get; set; } =
    [
        new Axis
        {
            MinLimit = 0,
            SeparatorsPaint = new SolidColorPaint(SKColors.DimGray),
            LabelsPaint = new SolidColorPaint(SKColors.Transparent),
        }
    ];

    [ObservableProperty]
    public partial long SentSpeed { get; set; } = 0;

    [ObservableProperty]
    public partial long ReceivedSpeed { get; set; } = 0;

    [ObservableProperty]
    public partial string MacAddressEntryText { get; set; } = "";

    [ObservableProperty]
    public partial int SelectedMacOuiVendorIndex { get; set; } = -1;

    [ObservableProperty]
    public partial bool Use02AsFirstOctetIsChecked { get; set; } = false;

    [ObservableProperty]
    public partial bool RestartConnectionOnApplyIsChecked { get; set; } = true;

    [ObservableProperty]
    public partial bool ReleaseIpAddressIsChecked { get; set; } = true;

    [ObservableProperty]
    public partial bool ApplyButtonIsEnabled { get; set; }

    [ObservableProperty]
    public partial bool RestoreButtonIsEnabled { get; set; }

    [RelayCommand]
    private async Task OnAppearingAsync()
    {
        RefreshNetworkAdapterDataGrid();
        await LoadMacOuiRegistryAsync();

        for (int i = 0; i < ADAPTER_SPEED_HISTORY_SIZE; i++)
        {
            SentSeries.Add(0);
            ReceivedSeries.Add(0);
        }

        var sentLineSeries = new LineSeries<long>
        {
            Values = SentSeries,
            Name = "Sent",
            GeometrySize = 0,
            LineSmoothness = 0,
            Stroke = new SolidColorPaint(new SKColor(172, 153, 234)) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(new SKColor(172, 153, 234, 32)),
        };
        Series.Add(sentLineSeries);

        var receivedLineSeries = new LineSeries<long>
        {
            Values = ReceivedSeries,
            Name = "Received",
            GeometrySize = 0,
            LineSmoothness = 0,
            Stroke = new SolidColorPaint(SKColors.SteelBlue) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(SKColors.SteelBlue.WithAlpha(32)),
        };
        Series.Add(receivedLineSeries);

        await MeasureNetworkSpeeds();
    }

    private async Task MeasureNetworkSpeeds()
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        //if (SelectedAdapter is null)
        //    return;

        long totalBytesSent;
        long totalBytesReceived;

        while (await periodicTimer.WaitForNextTickAsync())
        {
            if (SelectedAdapter is null || SelectedAdapter.OperationalStatus != "Up")
                continue;

            totalBytesSent = networkAdapterService.GetNetworkAdapterBytesSent(SelectedAdapter);
            totalBytesReceived = networkAdapterService.GetNetworkAdapterBytesReceived(SelectedAdapter);

            long uploadSpeed = networkAdapterService.GetNetworkAdapterBytesSent(SelectedAdapter) - totalBytesSent;
            long downloadSpeed = networkAdapterService.GetNetworkAdapterBytesReceived(SelectedAdapter) - totalBytesReceived;
            
            SentSeries.RemoveAt(0);
            ReceivedSeries.RemoveAt(0);

            SentSeries.Add(uploadSpeed);
            ReceivedSeries.Add(downloadSpeed);

            SentSpeed = uploadSpeed;
            ReceivedSpeed = downloadSpeed;
        }
    }

    private async Task LoadMacOuiRegistryAsync()
    {
        IEnumerable<MacOuiRegistrant> macOuiRegistrants = await macOuiRegistryService.GetRegistrantsAsync();

        MacOuiRegistry.AddRange(macOuiRegistrants);
    }

    [RelayCommand]
    private void RefreshNetworkAdapterDataGrid()
    {
        IEnumerable<NetworkAdapter> networkAdapters = networkAdapterService.GetNetworkAdapters();
        NetworkAdapters = new ObservableRangeCollection<NetworkAdapter>(networkAdapters);

        if (NetworkAdapters.Count > 0)
            SelectedAdapter = NetworkAdapters[0];
    }

    [RelayCommand]
    private void NetworkAdapterSelected()
    {
        if (SelectedAdapter is null) return;

        Use02AsFirstOctetIsChecked = networkAdapterService.IsNetworkAdapterWireless(SelectedAdapter);
        RestoreButtonIsEnabled = SelectedAdapter.IsMacChanged;

        ReceivedSeries.Clear();
        SentSeries.Clear();

        for (int i = 0; i < ADAPTER_SPEED_HISTORY_SIZE; i++)
        {
            ReceivedSeries.Add(0);
            SentSeries.Add(0);
        }
    }

    [RelayCommand]
    private void GenerateRandomMac()
    {
        if (Use02AsFirstOctetIsChecked)
        {
            char[] hexChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];
            int randomIndex;

            string newMacAddress = "02";

            for (int i = 2; i < 12; i++)
            {
                randomIndex = Random.Shared.Next(0, hexChars.Length);
                char randomChar = hexChars[randomIndex];

                newMacAddress += randomChar;
            }

            MacAddressEntryText = newMacAddress;
        }
        else
        {
            SelectedMacOuiVendorIndex = Random.Shared.Next(0, MacOuiRegistry.Count);
        }
    }

    [RelayCommand]
    private void MacOuiVendorSelected()
    {
        if (SelectedMacOuiVendorIndex < 0) return;

        if (Use02AsFirstOctetIsChecked) Use02AsFirstOctetIsChecked = false;

        string newMacAddress = MacOuiRegistry[SelectedMacOuiVendorIndex].Assignment!;

        char[] hexChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];
        int randomIndex;

        for (int i = 6; i < 12; i++)
        {
            randomIndex = Random.Shared.Next(0, hexChars.Length);
            char randomChar = hexChars[randomIndex];

            newMacAddress += randomChar;
        }

        MacAddressEntryText = newMacAddress;
    }

    [RelayCommand]
    private void Use02AsFirstOctetChanged()
    {
        if (Use02AsFirstOctetIsChecked)
        {
            if (MacAddressEntryText.Length < 2)
                MacAddressEntryText = "02";
            else
                MacAddressEntryText = "02" + MacAddressEntryText[2..];
        }
        else if (SelectedMacOuiVendorIndex < 0)
        {
            if (MacAddressEntryText.Length < 2)
                MacAddressEntryText = "00";
            else
                MacAddressEntryText = "00" + MacAddressEntryText[2..];
        }
    }

    [RelayCommand]
    private void MacAddressEntryTextChanged(TextChangedEventArgs e)
    {
        string macAddress = e.NewTextValue.Replace(" ", "").Replace("-", "");

        // Validate that the Mac Address contains only valid hex digits
        foreach (char c in macAddress)
        {
            if (!Uri.IsHexDigit(c))
            {
                MacAddressEntryText = e.OldTextValue;
                return;
            }
        }

        ApplyButtonIsEnabled = macAddress.Length == 12;

        // Update the OUI Vendor picker to be empty if the OUI portion of the Mac Address does not match the selected vendor
        if (SelectedMacOuiVendorIndex < 0) return;

        string vendorOui = MacOuiRegistry[SelectedMacOuiVendorIndex].Assignment ?? string.Empty;

        if (!macAddress.StartsWith(vendorOui))
            SelectedMacOuiVendorIndex = -1;
    }

    [RelayCommand]
    private async Task ApplyMacAddressAsync()
    {
        if (SelectedAdapter is null)
            return;

        ApplyButtonIsEnabled = false;

        // Validate MacAddressEntryText
        string newMacAddress = MacAddressEntryText.Replace(" ", "").Replace("-", "");

        bool successful = await networkAdapterService.SetNetworkAdapterMacAddressAsync(
            SelectedAdapter, newMacAddress, RestartConnectionOnApplyIsChecked, ReleaseIpAddressIsChecked);

        if (successful)
        {
            var networkAdapter = NetworkAdapters.FirstOrDefault(n => n.Id == SelectedAdapter.Id);
            networkAdapter?.CurrentMacAddress = NetworkAdapter.ConvertMacAddressToString(newMacAddress);
            networkAdapter?.IsMacChanged = networkAdapter.CurrentMacAddress != networkAdapter.OriginalMacAddress;

            await alertService.ShowAlertAsync(title: SelectedAdapter.Name, "MAC address changed successfully");
        }

        ApplyButtonIsEnabled = true;
    }

    [RelayCommand]
    private async Task RestoreOriginalMacAddressAsync()
    {
        if (SelectedAdapter is null)
            return;

        RestoreButtonIsEnabled = false;

        string newMacAddress = "";
        bool successful = await networkAdapterService.SetNetworkAdapterMacAddressAsync(
            SelectedAdapter, newMacAddress, RestartConnectionOnApplyIsChecked, ReleaseIpAddressIsChecked);

        if (successful)
        {
            var networkAdapter = NetworkAdapters.FirstOrDefault(n => n.Id == SelectedAdapter.Id);
            networkAdapter?.CurrentMacAddress = networkAdapter.OriginalMacAddress;
            networkAdapter?.IsMacChanged = networkAdapter.CurrentMacAddress != networkAdapter.OriginalMacAddress;

            await alertService.ShowAlertAsync(title: SelectedAdapter.Name, "MAC address restored successfully");
        }
    }    
}
