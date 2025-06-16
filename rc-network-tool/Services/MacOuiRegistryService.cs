using CsvHelper;
using rc_network_tool.Models;
using System.Diagnostics;
using System.Globalization;

namespace rc_network_tool.Services;

internal class MacOuiRegistryService : IMacOuiRegistryService
{
    private readonly HttpClient _httpClient;

    public MacOuiRegistryService()
    {
        _httpClient = new();
    }

    /// <summary>
    /// Retrieves the vendor MAC OUI registrants from a local file or downloads it from the web if the local file does not exist.
    /// </summary>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Code readability")]
    public async Task<IEnumerable<MacOuiRegistrant>> GetRegistrantsAsync()
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "mac_oui_registrants.csv");
        var fileInfo = new FileInfo(filePath);

        // If the local file does not exist, download the file from the URL
        if (!fileInfo.Exists)
            await DownloadFileFromWebAsync(filePath);

        // Read the file and deserialize its contents to MacOuiRegistrants List
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<MacOuiRegistrant>().ToList();
    }

    /// <summary>
    /// Downloads the vendor MAC OUI registrants file from the web and saves it to the specified file path, <paramref name="destinationPath"/>.
    /// </summary>
    /// <remarks>This method should only be called if the file does not exist or is older than 1 day.</remarks>
    /// <param name="destinationPath"></param>
    public async Task DownloadFileFromWebAsync(string destinationPath)
    {
        const string URL = @"https://regauth.standards.ieee.org/standards-ra-web/rest/assignments/download/?registry=MA-L&text=&filterType=all";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(URL);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("ERROR: Unable to download Vendor MAC OUI registrants. Status code: {0}", response.StatusCode);
                return;
            }

            await File.WriteAllTextAsync(destinationPath, await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ERROR {0}", ex.Message);
        }
    }
}
