namespace ASK.LiveCompose.Configuration;

public class LiveComposeConfig
{
    public string? BasePath { get; set; }
    public string? Key { get; set; }

    public bool EnableRateLimit { get; set; }
    public int RateLimit { get; set; }
    public int RateLimitQueueSize { get; set; }
    public int RateDelaySecond { get; set; }
}