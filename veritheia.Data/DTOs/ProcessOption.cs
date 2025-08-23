namespace Veritheia.Data.DTOs;

/// <summary>
/// Process option for UI display
/// </summary>
public class ProcessOption
{
    public string Type { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public string Requirements { get; set; } = "";
}
