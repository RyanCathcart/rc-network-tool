using Microsoft.Win32;
using rc_network_tool.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace rc_network_tool.Services;

internal class NetworkAdapterService : INetworkAdapterService
{
    const string REGISTRY_BASE_KEY = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
    const string REGISTRY_VALUE_MAC = "NetworkAddress";
    const string REGISTRY_VALUE_ORIG_MAC = "OriginalNetworkAddress";

    public long GetNetworkAdapterBytesReceived(NetworkAdapter adapter)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == adapter.Id);

        return networkInterface?.GetIPv4Statistics().BytesReceived ?? 0;
    }

    public long GetNetworkAdapterBytesSent(NetworkAdapter adapter)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == adapter.Id);

        return networkInterface?.GetIPv4Statistics().BytesSent ?? 0;
    }

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
            var currentMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue(REGISTRY_VALUE_MAC)?.ToString() ?? networkInterface?.GetPhysicalAddress().ToString());
            var originalMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue(REGISTRY_VALUE_ORIG_MAC)?.ToString() ?? currentMacAddress);
            if (string.IsNullOrEmpty(currentMacAddress))
                currentMacAddress = originalMacAddress;
            var isMacChanged = currentMacAddress != originalMacAddress;
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
                    IsMacChanged = isMacChanged,
                    Speed = speed,
                    OperationalStatus = operationalStatus
                });
            }
        }

        return networkAdapters;
    }

    public async Task<bool> SetNetworkAdapterMacAddressAsync(NetworkAdapter adapter, string newMacAddress, 
        bool restartAdapterIsEnabled, bool releaseIpAddressIsEnabled)
    {
        if (adapter is null) 
            return false;

        using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_KEY, writable: true); // Cannot write to registry without admin privileges

        if (registryKey is null) 
            return false;

        foreach (string subKeyName in registryKey.GetSubKeyNames())
        {
            if (!int.TryParse(subKeyName, out _))
                continue;

            using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName, writable: true);

            if (subKey is null) 
                continue;

            string? id = subKey.GetValue("NetCfgInstanceId")?.ToString();

            if (id != adapter.Id) 
                continue;

            if (adapter.CurrentMacAddress is null)
                return false;

            if (string.IsNullOrEmpty(newMacAddress))
            {
                subKey.DeleteValue(REGISTRY_VALUE_MAC);
                subKey.DeleteValue(REGISTRY_VALUE_ORIG_MAC);
            }
            else
            {
                if (subKey.GetValue(REGISTRY_VALUE_ORIG_MAC) is null)
                    subKey.SetValue(REGISTRY_VALUE_ORIG_MAC, adapter.CurrentMacAddress, RegistryValueKind.String);

                subKey.SetValue(REGISTRY_VALUE_MAC, newMacAddress, RegistryValueKind.String);
            }

            if (restartAdapterIsEnabled && adapter.Name is not null)
            {
                if (releaseIpAddressIsEnabled)
                    await ReleaseIpAddressAsync(adapter.Name);
                
                await DisableAdapterAsync(adapter.Name);
                await EnableAdapterAsync(adapter.Name);
            }

            return true;
        }

        return false;
    }

    private static async Task ReleaseIpAddressAsync(string interfaceName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ipconfig",
            Arguments = $"/release \"{interfaceName}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        await process.WaitForExitAsync();
    }

    private static async Task EnableAdapterAsync(string interfaceName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface set interface \"{interfaceName}\" enable",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        await process.WaitForExitAsync();
    }

    private static async Task DisableAdapterAsync(string interfaceName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface set interface \"{interfaceName}\" disable",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = psi };
        process.Start();
        await process.WaitForExitAsync();
    }

    public bool IsNetworkAdapterWireless(NetworkAdapter adapter)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == adapter.Id);

        return networkInterface?.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
    }
}
