namespace TaskManager.Api.Configuration;

public class JwtOptions
{
    public const string SectionName = "JWT";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; }
}
