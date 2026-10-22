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

On a configured route, select the terminal icon: **RCON console**. Enable RCON, enter its internal port (default **25575**) and password, then save. The same page tests the connection and sends commands such as `list`, displaying the server's response. Enable RCON in Minecraft's `server.properties` first.

API clients send the selected proxy ID, backend password, and command to the same HTTPS API address. Different domains share that one entry point; every backend can use internal port 25575. No public RCON listener is opened. Ordinary native RCON clients cannot connect to this HTTPS relay directly.

The password is remembered only in this browser, with an eye button to show/hide it. Clear it and save to forget it. No password column or saved server-side credential is added. Output stays on the page and clears on reload. The UI uses its existing session/CSRF protection; external clients can use an API key from **App Settings**. Use HTTPS or a trusted private network. The separate backend RCON connection is plaintext.

The original [ProxyCraft](https://github.com/DragoQC/ProxyCraft) stays focused on Minecraft routing. Plus is a separate application with its own database, cookie identity, settings, and keys. Do not point it at ProxyCraft's live data directory. RconCraft's connection and console UI are now included in Plus; no separate RconCraft installation or discovery setup is needed.

## Included

- Search, add, edit, delete, and copy domains; mobile cards and desktop tables.
- One RCON action per route for connection settings, a password eye button, authentication checks, and command responses.
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
