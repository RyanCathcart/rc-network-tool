namespace rc_network_tool.Models;

public class NetworkAdapter
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? HardwareId { get; set; }
    public string? OriginalMacAddress { get; set; }
    public string? CurrentMacAddress { get; set; }
    public bool IsMacChanged => CurrentMacAddress == OriginalMacAddress;
    public long? Speed { get; set; }
    public string? OperationalStatus { get; set; }

    public static string ConvertMacAddressToString(string? input)
    {
        if (input == null)
            return string.Empty;

        if (input.Length == 17 && input[2] == '-' && input[5] == '-' && input[8] == '-' && input[11] == '-' && input[14] == '-')
            return input;

        if (input.Length != 12)
            return string.Empty;

        foreach (char c in input)
        {
            if (!char.IsLetterOrDigit(c))
                throw new FormatException("Input does not contain valid characters to be converted to a proper MAC Address");
        }

        return $"{input[..2]}-{input.Substring(2, 2)}-{input.Substring(4, 2)}-{input.Substring(6, 2)}-{input.Substring(8, 2)}-{input.Substring(10, 2)}";
    }
}
