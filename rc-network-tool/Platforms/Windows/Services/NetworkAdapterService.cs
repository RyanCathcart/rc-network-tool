using Microsoft.Win32;
using rc_network_tool.Models;
using System.Net.NetworkInformation;

namespace rc_network_tool.Services;

internal class NetworkAdapterService : INetworkAdapterService
{
    const string REGISTRY_BASE_KEY = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
    const string REGISTRY_VALUE_MAC_ADDRESS = "NetworkAddress";

    public IEnumerable<NetworkAdapter> GetNetworkAdapters()
    {
        List<NetworkAdapter> networkAdapters = [];

        using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_KEY);

        if (registryKey is null)
            return [];

        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (string subKeyName in registryKey.GetSubKeyNames())
        {
            if (int.TryParse(subKeyName, out _) == false)
                continue;
            
            using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName);

            if (subKey is null)
                continue;

            var id = subKey.GetValue("NetCfgInstanceId")?.ToString();

            NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == id);

            var name = networkInterface?.Name ?? string.Empty;
            var description = subKey.GetValue("DriverDesc")?.ToString();
            var hardwareId = subKey.GetValue("ComponentId")?.ToString();
            var originalMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue("OriginalNetworkAddress")?.ToString() ?? string.Empty);
            var currentMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue(REGISTRY_VALUE_MAC_ADDRESS)?.ToString());
            if (string.IsNullOrEmpty(currentMacAddress))
                currentMacAddress = originalMacAddress;
            var speed = networkInterface?.Speed;
            var operationalStatus = networkInterface?.OperationalStatus.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(originalMacAddress))
            {
                networkAdapters.Add(new NetworkAdapter
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    HardwareId = hardwareId,
                    OriginalMacAddress = originalMacAddress,
                    CurrentMacAddress = currentMacAddress,
                    Speed = speed,
                    OperationalStatus = operationalStatus
                });
            }
        }

        return networkAdapters;
    }

    public bool SetNetworkAdapterMacAddress(NetworkAdapter adapter, string newMacAddress, bool restartAdapter, bool? persistChange = true)
    {
        if (adapter is null) 
            return false;

        using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_KEY, true); // Access denied, cannot write to registry without admin privileges

        if (registryKey is null) 
            return false;

        foreach (string subKeyName in registryKey.GetSubKeyNames())
        {
            if (int.TryParse(subKeyName, out _) == false) 
                continue;

            using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName, true);

            if (subKey is null) 
                continue;

            string? id = subKey.GetValue("NetCfgInstanceId")?.ToString();

            if (id != adapter.Id) 
                continue;

            subKey.SetValue(REGISTRY_VALUE_MAC_ADDRESS, newMacAddress, RegistryValueKind.String);

            if (restartAdapter && adapter.Name is not null)
            {
                DisableAdapter(adapter.Name);
                EnableAdapter(adapter.Name);
            }

            return true;
        }

        return false;
    }

    private static void EnableAdapter(string interfaceName)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface set interface \"{interfaceName}\" enable",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var p = new System.Diagnostics.Process { StartInfo = psi };

        p.Start();
    }

    static void DisableAdapter(string interfaceName)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface set interface \"{interfaceName}\" disable",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var p = new System.Diagnostics.Process { StartInfo = psi };

        p.Start();
    }

    public bool IsNetworkAdapterWireless(NetworkAdapter adapter)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == adapter.Id);

        return networkInterface?.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
    }
}
