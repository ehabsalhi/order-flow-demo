namespace MainServer.Helpers;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicySettings Auth { get; set; } = new();
    public RateLimitPolicySettings Api { get; set; } = new();
}

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowMinutes { get; set; } = 1;
}
