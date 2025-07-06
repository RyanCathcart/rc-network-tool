using rc_network_tool.Models;

namespace rc_network_tool.Services;

public interface IMacOuiRegistryService
{
    /// <summary>
    /// Retrieves the vendor MAC OUI registrants from a local file or downloads it from the web if the local file does not exist.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<MacOuiRegistrant>> GetRegistrantsAsync();
}
