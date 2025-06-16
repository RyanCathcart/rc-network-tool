using CsvHelper.Configuration.Attributes;

namespace rc_network_tool.Models;

public class MacOuiRegistrant
{
    [Name("Assignment")]
    public string? Assignment { get; set; }

    [Name("Organization Name")]
    public string? Name { get; set; }

    [Name("Organization Address")]
    public string? Address { get; set; }

    public string Display => $"[{Assignment?[..2]}-{Assignment?.Substring(2, 2)}-{Assignment?[4..]}] {Name}";
}
