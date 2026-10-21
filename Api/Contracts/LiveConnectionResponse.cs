namespace ProxyCraftPlus.Api.Contracts;

public sealed record LiveConnectionResponse(
    long Id,
    string PlayerName,
    string Domain,
    DateTimeOffset ConnectedAt);
