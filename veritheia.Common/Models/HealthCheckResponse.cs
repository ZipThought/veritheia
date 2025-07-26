namespace veritheia.Common.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = "Unknown";
    public string Service { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Environment { get; set; } = string.Empty;
}

public class DetailedHealthCheckResponse : HealthCheckResponse
{
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, string> Dependencies { get; set; } = new();
}