using rc_network_tool.Models;

namespace rc_network_tool.Services;

public interface INetworkAdapterService
{
    IEnumerable<NetworkAdapter> GetNetworkAdapters();

    public Task<bool> SetNetworkAdapterMacAddressAsync(NetworkAdapter adapter, 
        string newMacAddress, bool restartAdapterIsEnabled, bool releaseIpAddressIsEnabled);

    bool IsNetworkAdapterWireless(NetworkAdapter adapter);
}
