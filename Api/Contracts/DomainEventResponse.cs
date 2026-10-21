namespace ProxyCraftPlus.Api.Contracts;

public sealed record DomainEventResponse(
    int ProxyId,
    string Domain,
    DateTimeOffset OccurredAt);
