namespace TaskManager.Api.Configuration;

public class DataProtectionApiOptions
{
    public const string SectionName = "DataProtection";

    public string AppName { get; set; } = string.Empty;

    public int KeyLifetimeDays { get; set; }
}
