using rc_network_tool.Models;

namespace rc_network_tool.Services;

public interface INetworkAdapterService
{
    /// <summary>
    /// Retrieves the network adapters available on the system with their relevant details.
    /// </summary>
    /// <returns>The IEnumerable of NetworkAdapter objects</returns>
    IEnumerable<NetworkAdapter> GetNetworkAdapters();

    /// <summary>
    /// Asynchronously sets the MAC address of the specified network adapter.
    /// </summary>
    /// <param name="adapter">The network adapter whose MAC address will be updated.</param>
    /// <param name="newMacAddress">The new MAC address to assign to the network adapter. Must be a valid MAC address format.</param>
    /// <param name="restartAdapterIsEnabled">A value indicating whether the network adapter should be restarted after the MAC address is updated.  Set to
    /// <see langword="true"/> to restart the adapter; otherwise, <see langword="false"/>.</param>
    /// <param name="releaseIpAddressIsEnabled">A value indicating whether the target network adapter's IP address should be released before updating the MAC address.  Set to <see
    /// langword="true"/> to release the IP address; otherwise, <see langword="false"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the MAC address
    /// was successfully updated;  otherwise, <see langword="false"/>.</returns>
    public Task<bool> SetNetworkAdapterMacAddressAsync(NetworkAdapter adapter, 
        string newMacAddress, bool restartAdapterIsEnabled, bool releaseIpAddressIsEnabled);

    /// <summary>
    /// Determines whether the specified network adapter is a wireless adapter.
    /// </summary>
    /// <param name="adapter">The network adapter to evaluate.</param>
    /// <returns><see langword="true"/> if the specified adapter is a wireless adapter; otherwise, <see langword="false"/>.</returns>
    bool IsNetworkAdapterWireless(NetworkAdapter adapter);

    /// <summary>
    /// Retrieves the total number of bytes sent by the specified network adapter since it was initialized or last reset.
    /// </summary>
    /// <remarks>Ensure the <paramref name="adapter"/> is properly configured and active before calling this
    /// method.</remarks>
    /// <param name="adapter">The <see cref="NetworkAdapter"/> instance representing the network adapter for which to retrieve the bytes sent.</param>
    /// <returns>The total number of bytes sent by the specified network adapter.</returns>
    public long GetNetworkAdapterBytesSent(NetworkAdapter adapter);

    /// <summary>
    /// Retrieves the total number of bytes received by the specified network adapter since it was initialized or last reset.
    /// </summary>
    /// <remarks>Ensure the <paramref name="adapter"/> is properly configured and active before calling this
    /// method.</remarks>
    /// <param name="adapter">The <see cref="NetworkAdapter"/> instance representing the network adapter for which to retrieve the bytes received.</param>
    /// <returns>The total number of bytes received by the specified network adapter.</returns>
    public long GetNetworkAdapterBytesReceived(NetworkAdapter adapter);
}
