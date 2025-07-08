using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rc_network_tool.Models;
using rc_network_tool.Services;
using rc_network_tool.Utils;

namespace rc_network_tool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INetworkAdapterService _networkAdapterService;
    private readonly IMacOuiRegistryService _macOuiRegistryService;

    public MainViewModel(INetworkAdapterService networkAdapterService, IMacOuiRegistryService macOuiRegistryService)
    {
        _networkAdapterService = networkAdapterService;
        _macOuiRegistryService = macOuiRegistryService;

        RefreshNetworkAdapterDataGrid();
    }

    [ObservableProperty]
    public partial ObservableRangeCollection<NetworkAdapter> NetworkAdapters { get; set; } = [];

    [ObservableProperty]
    public partial ObservableRangeCollection<MacOuiRegistrant> MacOuiRegistry { get; set; } = [];

    [ObservableProperty]
    public partial NetworkAdapter? SelectedAdapter { get; set; } = null;

    [ObservableProperty]
    public partial string MacAddressEntryText { get; set; } = "";

    [ObservableProperty]
    public partial int SelectedMacOuiVendorIndex { get; set; } = -1;

    [ObservableProperty]
    public partial bool Use02AsFirstOctetIsEnabled { get; set; } = false;
    
    [ObservableProperty]
    public partial bool RestartConnectionOnApplyIsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool ReleaseIpAddressIsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool ApplyButtonIsEnabled { get; set; }

    [ObservableProperty]
    public partial bool RestoreButtonIsEnabled { get; set; }

    [RelayCommand]
    private void RefreshNetworkAdapterDataGrid()
    {
        IEnumerable<NetworkAdapter> networkAdapters = _networkAdapterService.GetNetworkAdapters();
        NetworkAdapters = new ObservableRangeCollection<NetworkAdapter>(networkAdapters);

        if (NetworkAdapters.Count > 0)
            SelectedAdapter = NetworkAdapters[0];
    }

    [RelayCommand]
    private void NetworkAdapterSelected()
    {
        if (SelectedAdapter is null) return;

        Use02AsFirstOctetIsEnabled = _networkAdapterService.IsNetworkAdapterWireless(SelectedAdapter);
    }

    [RelayCommand]
    private void GenerateRandomMac()
    {
        if (Use02AsFirstOctetIsEnabled)
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

        if (Use02AsFirstOctetIsEnabled) Use02AsFirstOctetIsEnabled = false;

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
        if (Use02AsFirstOctetIsEnabled)
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

        // Validate MacAddressEntryText
        string newMacAddress = MacAddressEntryText.Replace(" ", "").Replace("-", "");

        if (newMacAddress.Length != 12) 
            return;

        bool successful = await _networkAdapterService.SetNetworkAdapterMacAddressAsync(SelectedAdapter, newMacAddress, RestartConnectionOnApplyIsEnabled, ReleaseIpAddressIsEnabled);

        if (successful)
        {
            NetworkAdapters.Where(n => n.Id == SelectedAdapter.Id)
                           .FirstOrDefault()?.CurrentMacAddress = NetworkAdapter.ConvertMacAddressToString(newMacAddress);
        }
    }

    [RelayCommand]
    private async Task RestoreOriginalMacAddressAsync()
    {
        if (SelectedAdapter is null)
            return;
        string newMacAddress = "";
        bool successful = await _networkAdapterService.SetNetworkAdapterMacAddressAsync(SelectedAdapter, newMacAddress, RestartConnectionOnApplyIsEnabled, ReleaseIpAddressIsEnabled);

        if (successful)
        {
            NetworkAdapters.Where(n => n.Id == SelectedAdapter.Id)
                           .FirstOrDefault()?.CurrentMacAddress = NetworkAdapter.ConvertMacAddressToString(newMacAddress);
        }
    }

    [RelayCommand]
    private async Task OnAppearingAsync()
    {
        // Load the Mac OUI registry when the view model appears
        await LoadMacOuiRegistryAsync().ConfigureAwait(false);
    }

    private async Task LoadMacOuiRegistryAsync()
    {
        IEnumerable<MacOuiRegistrant> macOuiRegistrants = await _macOuiRegistryService.GetRegistrantsAsync();

        MacOuiRegistry.AddRange(macOuiRegistrants);
    }
}
