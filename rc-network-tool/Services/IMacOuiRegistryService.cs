using rc_network_tool.Models;

namespace rc_network_tool.Services;

public interface IMacOuiRegistryService
{
    Task<IEnumerable<MacOuiRegistrant>> GetRegistrantsAsync();
}
