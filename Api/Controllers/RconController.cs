using Microsoft.AspNetCore.Mvc;
using ProxyCraftPlus.Api.Contracts;
using ProxyCraftPlus.Services;

namespace ProxyCraftPlus.Api.Controllers;

[ApiController, Route("api/v1/proxies/{id:int}/rcon"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RconController(ProxyService proxies, RconRelayService relay) : ControllerBase
{
    [HttpPut]
    [EndpointSummary("Configure this route's RCON relay")]
    [EndpointDescription("Enables the optional relay to the saved Minecraft destination host and backend RCON port. Opens no public RCON socket and never stores a password. Minecraft sessions remain connected.")]
    [ProducesResponseType<ProxyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProxyResponse>> Configure(int id, RconSettingsRequest request, CancellationToken cancellationToken)
    {
        ProxyResponse? response = await proxies.UpdateRconAsync(id, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("check")]
    [EndpointSummary("Check backend RCON authentication")]
    [EndpointDescription("Accepts the dashboard cookie/CSRF pair or a valid X-API-Key under the existing management rules. Authenticates to this enabled route's backend without executing a command. Password is request-only; use HTTPS or a trusted private network.")]
    [ProducesResponseType<RconCheckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public Task<ActionResult<RconCheckResponse>> Check(int id, RconCheckRequest request, CancellationToken cancellationToken) =>
        HandleAsync(() => relay.CheckAsync(id, request.Password, cancellationToken));

    [HttpPost("commands")]
    [EndpointSummary("Relay one RCON command")]
    [EndpointDescription("Accepts the same dashboard cookie/CSRF or API-key authorization as management. Resolves the selected route to its saved backend, authenticates with the request password, sends the command once, and returns bounded text output. No credentials, commands, or output are persisted by the server. No automatic command retry.")]
    [ProducesResponseType<RconCommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public Task<ActionResult<RconCommandResponse>> Execute(int id, RconCommandRequest request, CancellationToken cancellationToken) =>
        HandleAsync(() => relay.ExecuteAsync(id, request, cancellationToken));

    private async Task<ActionResult<T>> HandleAsync<T>(Func<Task<T?>> action) where T : class
    {
        try
        {
            T? response = await action();
            return response is null ? NotFound() : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: exception.Message);
        }
        catch (ProxyConflictException exception)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: exception.Message);
        }
        catch (RconException exception)
        {
            return Problem(statusCode: StatusCodes.Status502BadGateway, title: exception.Message);
        }
    }
}
