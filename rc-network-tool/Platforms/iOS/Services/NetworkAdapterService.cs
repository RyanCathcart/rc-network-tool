﻿using rc_network_tool.Models;

namespace rc_network_tool.Services;

internal class NetworkAdapterService : INetworkAdapterService
{
    public long GetNetworkAdapterBytesReceived(NetworkAdapter adapter)
    {
        throw new NotImplementedException();
    }

    public long GetNetworkAdapterBytesSent(NetworkAdapter adapter)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<NetworkAdapter> GetNetworkAdapters()
    {
        throw new NotImplementedException();
    }

    public bool IsNetworkAdapterWireless(NetworkAdapter adapter)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetNetworkAdapterMacAddressAsync(NetworkAdapter adapter, string newMacAddress, bool restartAdapterIsEnabled, bool releaseIpAddressIsEnabled)
    {
        throw new NotImplementedException();
    }
}
