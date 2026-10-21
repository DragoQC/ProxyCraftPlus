# ProxyCraftPlus

**ProxyCraft's simple Minecraft routing, with an optional RCON API relay.**

## Install

On **Debian 13**, run as root:

```bash
curl -fsSL https://raw.githubusercontent.com/DragoQC/ProxyCraftPlus/main/install.sh | bash
```

Open **http://SERVER_IP:8080**. Rerun to update; saved data survives. Requirements: **1 vCPU, 1 GB RAM, 5 GB disk** (10 GB recommended). The native installer supports Debian 13/systemd; the original deployment pattern was tested in a Debian 13 LXC and Docker. Plus's renamed installer and Docker files have build/configuration checks; a fresh Plus host installation has not yet been tested.

## One gate for your servers

Point your Minecraft domains at this instance and forward **TCP 25565** to it. Add each domain and its backend address in **Configured proxies**. The destination can be on your LAN or elsewhere.

ProxyCraftPlus routes existing **Minecraft Java Edition** servers. It does not host worlds or manage Minecraft installations.

## Optional RCON, without extra public ports

On a configured route, select the terminal icon: **Set up RCON**. Enable the relay and enter that server's internal RCON port (default **25575**). Enable RCON and set its password in Minecraft's `server.properties` first.

API clients send the selected proxy ID, backend password, and command to the same HTTPS API address. Different domains share that one entry point; every backend can use internal port 25575. No public RCON listener is opened. Ordinary native RCON clients cannot connect to this HTTPS relay directly.

Commands and backend passwords are request-only: Plus does not store them, schedule them, or add client tools. Create an API key in **App Settings**; a valid key is always required for relay calls, including with dashboard sign-in off. Use HTTPS, or a trusted private network. The separate backend RCON connection is plaintext.

The original [ProxyCraft](https://github.com/DragoQC/ProxyCraft) stays focused on Minecraft routing. Plus is a separate application with its own database, cookie identity, settings, and keys. Do not point it at ProxyCraft's live data directory. RconCraft remains the separate client application; its API-relay transport is not included in this repository.

## Included

- Search, add, edit, delete, and copy domains; mobile cards and desktop tables.
- Route health, live connection counts, and the latest 20 player connection attempts per route.
- Optional sign-in, authenticator 2FA, API keys, and a public welcome dashboard.
- CPU/RAM/disk cards and Light, Dark, and Rainbow themes.
- SQLite initialized on launch. No Node/npm or manual database setup.

Fresh installs start without login. Enable authentication in App Settings before exposing management beyond your trusted network.

## Develop

```bash
dotnet restore --locked-mode
dotnet watch --launch-profile http
```

Open **http://localhost:5380**. After changing Tailwind classes, run `dotnet build -t:BuildCss --no-restore` once. Stack: **.NET 10 MVC**, **EF Core SQLite 10.0.12**, **Tailwind 4.3.3**.

Docker: `docker compose up -d --build` publishes web port 8080 and Minecraft port 25565. No additional RCON port mappings are needed. Plus and original ProxyCraft need separate machines/IPs or different listener ports when run together.

[API](docs/API.md) · [Installation](docs/INSTALLATION.md) · [Security](docs/SECURITY.md) · [Architecture](docs/ARCHITECTURE.md) · [Roadmap](docs/ROADMAP.md)

**License:** [MIT](LICENSE). Free to use, modify, share, and sell; keep the copyright and license notice.
