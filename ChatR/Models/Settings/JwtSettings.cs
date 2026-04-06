namespace ChatR.Models.Settings;

public class JwtSettings
{
    public string? Secret { get; set; } = Environment.GetEnvironmentVariable("JWT_SECRET");
    public string Issuer { get; set; } = "ChatR";
    public string Audience { get; set; } = "ChatR";
    public int TokenLifetimeMinutes { get; set; } = 60;
}
