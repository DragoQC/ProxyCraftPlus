using System.ComponentModel.DataAnnotations;

namespace ProxyCraftPlus.Api.Contracts;

public sealed class RconSettingsRequest
{
    public bool Enabled { get; set; }

    [Range(1, 65535)]
    public int DestinationPort { get; set; } = 25575;
}

public sealed class RconCheckRequest
{
    [Required, StringLength(1024)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RconCommandRequest
{
    [Required, StringLength(1024)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(1024)]
    public string Command { get; set; } = string.Empty;
}

public sealed record RconCheckResponse(bool Connected, DateTimeOffset CheckedAt);

public sealed record RconCommandResponse(string Output, bool Truncated, DateTimeOffset CompletedAt);
