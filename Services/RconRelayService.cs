using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProxyCraftPlus.Api.Contracts;
using ProxyCraftPlus.Data;
using ProxyCraftPlus.Models;

namespace ProxyCraftPlus.Services;

public sealed class RconRelayService(IServiceScopeFactory scopes, RconClient client)
{
    private readonly SemaphoreSlim _connections = new(8, 8);

    public async Task<RconCheckResponse?> CheckAsync(int id, string password, CancellationToken cancellationToken) =>
        await RelayAsync(id, password, null, cancellationToken) is null ? null : new RconCheckResponse(true, DateTimeOffset.UtcNow);

    public Task<RconCommandResponse?> ExecuteAsync(int id, RconCommandRequest request, CancellationToken cancellationToken) =>
        RelayAsync(id, request.Password, request.Command, cancellationToken);

    private async Task<RconCommandResponse?> RelayAsync(int id, string password, string? command, CancellationToken cancellationToken)
    {
        if (password.Length == 0 || password.Contains('\0') || Encoding.UTF8.GetByteCount(password) > 1024)
        {
            throw new ArgumentException("Enter an RCON password of up to 1024 UTF-8 bytes without null characters.");
        }
        if (command is not null)
        {
            command = command.Trim();
            if (command.Length == 0 || command.Any((char character) => char.IsControl(character)) || Encoding.UTF8.GetByteCount(command) > 1024)
            {
                throw new ArgumentException("Enter one command, up to 1024 UTF-8 bytes, without control characters.");
            }
        }
        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        ProxyCraftPlusDbContext database = scope.ServiceProvider.GetRequiredService<ProxyCraftPlusDbContext>();
        Proxy? proxy = await database.Proxies.AsNoTracking().SingleOrDefaultAsync((Proxy route) => route.Id == id, cancellationToken);
        if (proxy is null)
        {
            return null;
        }
        if (!proxy.IsActive || !proxy.RconEnabled)
        {
            throw new ProxyConflictException("RCON relay is disabled for this route.");
        }
        if (!await _connections.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
        {
            throw new RconException("RCON relay is busy. Try again shortly.");
        }
        try
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(command is null ? 3 : 10));
            if (command is null)
            {
                await client.CheckAsync(proxy.DestinationHost, proxy.RconDestinationPort, password, timeout.Token);
                return new RconCommandResponse(string.Empty, false, DateTimeOffset.UtcNow);
            }
            return await client.ExecuteAsync(proxy.DestinationHost, proxy.RconDestinationPort, password, command, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RconException(command is null ? "The backend RCON check timed out." : "RCON timed out. The command may have executed; check before retrying.");
        }
        catch (Exception exception) when (exception is IOException or SocketException)
        {
            throw new RconException(command is null ? "Unable to reach the backend RCON server." : "The RCON connection failed. The command may have executed; commands are never retried automatically.");
        }
        finally
        {
            _connections.Release();
        }
    }
}
