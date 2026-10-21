namespace ProxyCraftPlus.Configuration;

public sealed record ApplicationSettings
{
    public const string DefaultPublicDashboardTitle = "Welcome to ProxyCraftPlus";

    public const string DefaultPublicDashboardMessage = "The Minecraft proxy that just works.";

    public bool AuthenticationEnabled { get; init; }

    public bool PublicDashboardEnabled { get; init; }

    public bool ShowPublicDomains { get; init; }

    public string PublicDashboardMessage { get; init; } = DefaultPublicDashboardMessage;

    public string PublicDashboardTitle { get; init; } = DefaultPublicDashboardTitle;

    public int ListenPort { get; init; } = 25565;
}
