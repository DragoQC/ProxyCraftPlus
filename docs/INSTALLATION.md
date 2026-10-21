# Installation

The native installer targets Debian 13 with systemd on amd64 or arm64. The inherited ProxyCraft deployment pattern was verified in a Debian 13 amd64 LXC. Plus has passed simulated-host installer checks and a real non-root Docker run with persistence across container replacement. A fresh native Plus installation and other distributions/architectures have not yet been verified.

## Debian 13

Run this one-liner as root inside a Debian 13 VM/LXC:

```bash
curl -fsSL https://raw.githubusercontent.com/DragoQC/ProxyCraftPlus/main/install.sh | bash
```

Use `sudo bash` in place of `bash` when running from an ordinary user account. If curl is missing, install it first with `apt-get update && apt-get install -y curl ca-certificates`.

The installer always downloads [DragoQC/ProxyCraftPlus](https://github.com/DragoQC/ProxyCraftPlus), defaulting to `main`. It does not require a checkout or repository/source option, create an LXC/VM, or change Proxmox, DNS, router, firewall, or TLS configuration.

The script installs `ca-certificates`, `curl`, `tar`, and `gzip`. If `/usr/bin/dotnet` lacks a .NET 10 SDK, it adds Microsoft's Debian 13 package feed and installs `dotnet-sdk-10.0`, which includes the ASP.NET Core runtime. Builds run as the dedicated `proxycraftplus` account with temporary caches; Node/npm and the Tailwind compiler are not installed. The included compiled CSS is used.

| Path | Purpose |
| --- | --- |
| `/opt/proxycraftplus/releases/` | Root-owned published app releases |
| `/opt/proxycraftplus/current` | Symlink to the active release |
| `/var/lib/proxycraftplus/` | Persistent SQLite database, settings, and cookie-protection keys |
| `/etc/proxycraftplus/proxycraftplus.env` | Environment settings, readable by root and the service group |
| `/etc/systemd/system/proxycraftplus.service` | Service run as `proxycraftplus`, enabled at boot |

```bash
curl -fsSL https://raw.githubusercontent.com/DragoQC/ProxyCraftPlus/main/install.sh | bash -s -- --port 8081
curl -fsSL https://raw.githubusercontent.com/DragoQC/ProxyCraftPlus/main/install.sh | bash -s -- --ref TAG_OR_COMMIT
```

`--ref` selects a branch, tag, or commit from this repository. The script validates its options, Debian version, architecture, and systemd before installing packages. It refuses an existing non-symlink `/opt/proxycraftplus/current`.

Rerun the installer to update. It builds before changing the active release, preserves data and existing environment settings, and keeps the previous dashboard port unless `--port` is supplied. It switches releases using a symlink and checks that the service serves `/logo.svg`; on failed startup, it restores the previous release and environment where available. Old releases remain for manual rollback. An update restarts the process and disconnects active Minecraft sessions.

```bash
sudo systemctl restart proxycraftplus
sudo systemctl status proxycraftplus
sudo journalctl -u proxycraftplus -n 50 --no-pager
```

## Docker

`Dockerfile` builds with the official .NET 10 SDK and runs on the official ASP.NET Core 10 Ubuntu Noble image. It uses the existing lockfile and compiled CSS. The final image contains the published app/runtime, without the SDK, source checkout, Node/npm, or Tailwind compiler. Its default user is `app` (UID 1654), not root.

`docker compose up -d --build` builds `proxycraftplus:local`, maps TCP 8080 and 25565, and mounts the named `proxycraftplus-data` volume at `/var/lib/proxycraftplus`. No public registry image is published by these files.

Update from a refreshed checkout with:

```bash
docker compose up -d --build
```

The same volume preserves routes, accounts, settings, API keys, and cookie-protection keys across container recreation. With a bind mount instead, its directory must be writable by UID 1654. Back up the whole data directory/volume with the app stopped so SQLite and its journal files form a consistent copy. Container removal and volume removal are separate operations; keep the data volume when updating.

Port mappings must match the listener configured in Proxy Settings. For example, a container listener changed to 25566 can still be exposed to players on the default port with `25565:25566`.

Resource cards can only read limits exposed inside the container's cgroup namespace. Some Docker hosts enforce additional CPU limits in a parent group that is hidden from the container.

## Deployment configuration

| Environment variable | Supplied value / behavior |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_HTTP_PORTS` | `8080`; native installer can set another port with `--port` |
| `Storage__Directory` | `/var/lib/proxycraftplus` |
| `DataProtection__KeysDirectory` | `/var/lib/proxycraftplus/keys`; stable cookie keys and application name across releases |
| `Http__RedirectToHttps` | `false` in the supplied deployments; defaults to true for other production runs |

These deployments expose an HTTP dashboard for LAN access or an operator-configured HTTPS reverse proxy. They do not provision certificates. Minecraft traffic remains TCP, independent of dashboard TLS.

Fresh data starts without login, as configured in the app. Enable sign-in and generate an API key in App Settings when wanted. The installer does not add auth flags or reset existing credentials. No manual schema command is needed: the app initializes its existing SQLite schema at startup.

Official references: [Microsoft's Debian 13 .NET instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian#debian-13), [.NET container images](https://github.com/dotnet/dotnet-docker), [multi-stage Docker builds](https://docs.docker.com/build/building/multi-stage/), and [persisting ASP.NET Core cookie-protection keys](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0#persist-keys-to-the-file-system-persistkeystofilesystem).

## Plus isolation

Plus uses its own `proxycraftplus` service, `/var/lib/proxycraftplus` data directory, and `proxycraftplus-data` Docker volume. Do not reuse a running original ProxyCraft database or key ring. Run the apps on separate IPs or configure distinct dashboard/Minecraft ports if they share a machine. RCON relay uses the existing web/API port and opens no additional public TCP listener. The renamed native installer has simulated-host checks; a fresh native Plus install has not yet been verified on a separate Debian host.
