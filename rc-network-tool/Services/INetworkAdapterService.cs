using rc_network_tool.Models;

namespace rc_network_tool.Services;

public interface INetworkAdapterService
{
    IEnumerable<NetworkAdapter> GetNetworkAdapters();

    public bool SetNetworkAdapterMacAddress(NetworkAdapter adapter, string newMacAddress, bool restartAdapter, bool releaseIpAddress);

    bool IsNetworkAdapterWireless(NetworkAdapter adapter);
}
