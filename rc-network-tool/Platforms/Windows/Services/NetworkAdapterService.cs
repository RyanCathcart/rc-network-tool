using Microsoft.Win32;
using rc_network_tool.Models;
using System.Net.NetworkInformation;

namespace rc_network_tool.Services;

internal class NetworkAdapterService : INetworkAdapterService
{
    public IEnumerable<NetworkAdapter> GetNetworkAdapters()
    {
        const string REGISTRY_BASE_KEY = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";

        List<NetworkAdapter> networkAdapters = [];

        using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_KEY);

        if (registryKey == null)
            return [];

        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (string subKeyName in registryKey.GetSubKeyNames())
        {
            if (int.TryParse(subKeyName, out _) == false)
                continue;
            
            using RegistryKey? subKey = registryKey.OpenSubKey(subKeyName);

            if (subKey == null)
                continue;

            var id = subKey.GetValue("NetCfgInstanceId")?.ToString();

            NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == id);

            var name = networkInterface?.Name ?? string.Empty;
            var description = subKey.GetValue("DriverDesc")?.ToString();
            var hardwareId = subKey.GetValue("ComponentId")?.ToString();
            var originalMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue("OriginalNetworkAddress")?.ToString() ?? string.Empty);
            var currentMacAddress = NetworkAdapter.ConvertMacAddressToString(subKey.GetValue("NetworkAddress")?.ToString());
            if (string.IsNullOrEmpty(currentMacAddress))
                currentMacAddress = originalMacAddress;
            var speed = networkInterface?.Speed.ToString() ?? string.Empty;
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

    public bool IsNetworkAdapterWireless(NetworkAdapter adapter)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        NetworkInterface? networkInterface = networkInterfaces.SingleOrDefault(n => n.Id == adapter.Id);

        return networkInterface?.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
    }
}
