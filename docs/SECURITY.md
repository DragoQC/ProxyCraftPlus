# Security notes

ProxyCraftPlus's scope is domain-based Minecraft Java forwarding and a management UI/API. Certificate provisioning, Minecraft server administration, and player authentication remain with the operator's existing infrastructure and Minecraft servers.

## Management access

- **Enable sign-in before allowing untrusted access.** Fresh installations deliberately start without authentication. In that mode, anyone who can reach the dashboard can manage the app, including obtaining a browser session even when an API key is configured.
- Use a strong password and optional authenticator 2FA. Browser mutations require CSRF tokens; password and factor verification are rate limited.
- External clients send `X-API-Key` to the existing `/api/v1` endpoints. Keys grant full management access, are stored only as hashes, and are displayed once. Regeneration invalidates the previous key. Treat a key like an administrator password. See the [API reference](API.md).
- Keep the management port private on a trusted network or VPN. For access over an untrusted network, use HTTPS through your existing reverse proxy and restrict direct access to the HTTP port. Plain HTTP does not encrypt passwords, session cookies, or API keys. ProxyCraftPlus does not need to issue or renew certificates to use external HTTPS termination. [Microsoft's HTTPS guidance](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0) explains the transport requirement.
- Keep the database, settings, and Data Protection keys private and back them up together. Keep the host OS, .NET runtime, and dependencies patched. [Microsoft requires current patch releases](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) within a supported .NET release.

## Review on October 21, 2026

A focused source review and checks against a locally published Production build used temporary data, local echo backends, and Firefox. They did not inspect the production server or its network configuration.

The checks passed for:

- Domain selection, fragmented handshake/login forwarding without byte changes, bidirectional traffic, isolated route edits, and disconnect cleanup.
- Anonymous management denial with sign-in enabled, browser CSRF rejection, password sign-in, and HttpOnly/SameSite session cookies.
- Password-confirmed authenticator setup, session invalidation, TOTP replay rejection, and one-use recovery codes.
- API-key-only route creation, editing, and deletion; key rotation; spoofed browser-header rejection; and denial of factor-management requests using only a key.
- Opt-in public visibility, escaped welcome text, and exclusion of destinations, players, and management data from the public page.
- Invalid and oversized packet rejection, initial read timeouts, input validation, duplicate-destination rejection, and the 20-attempt history limit.
- Hashed stored credentials, protected authenticator secrets, authentication and route persistence, and login rate limiting.
- Desktop/mobile rendering, theme controls, the live dialog, domain-copy expiry, and API reference inputs/outputs.

Malformed Minecraft packets were also changed to follow the expected connection-rejection logging path, avoiding full error logs for routine invalid input.

The NuGet vulnerability check, including transitive packages, returned no reported vulnerable packages on this date. Local checks used .NET/ASP.NET Core **10.0.12** and EF Core SQLite **10.0.12**. Microsoft's [October 13 release](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.13/10.0.13.md) provides the current **10.0.13** runtime patch, including security fixes; deployed runtimes should stay current independently of NuGet results. The build, Release publish, and C# type-style verification passed.

This is a limited local review, not a penetration test or a guarantee that the app is free of vulnerabilities.

## Availability limits

The public Minecraft listener currently has no global or per-source connection cap. Packet bounds and read timeouts reduce individual connection costs, but connection floods can still exhaust sockets, memory, bandwidth, or database-write capacity. The saved-history limit bounds stored rows; it does not rate limit incoming login attempts. Apply network-level limits and monitoring appropriate to your exposure.

Live connection state and recorded player names describe what the proxy observed. Names are supplied by clients and do not prove authenticated identities or successful Minecraft joins.

## Optional RCON API relay

Relay checks and commands require a valid API key even when sign-in is disabled. Each route must opt in. The destination is read from the saved route; requests cannot supply an arbitrary target host or port. The main account/API key can still change routes through authorized management operations.

Use HTTPS or a trusted private transport for the API. Native backend RCON remains plaintext and belongs on a trusted LAN or VPN. No public RCON sockets, backend password store, command audit database, schedules, or automatic retries are included. Do not enable body logging in reverse proxies or custom middleware for these endpoints.

Eight backend connections, five-second queue waits, bounded request/packet/output sizes, and operation deadlines limit relay resource use. They are not a general denial-of-service guarantee. A failed/timed-out command may have executed; investigate before retrying. In-flight calls may complete after configuration changes.

The independent Plus relay is validated with temporary protocol fixtures and browser checks. The original ProxyCraft review above describes inherited code and does not constitute a complete security audit of Plus.
