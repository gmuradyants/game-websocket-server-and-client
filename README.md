# Game WebSocket Server and Client

![.NET 6](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

A real-time game backend and interactive console client written in C# on .NET 6, communicating over WebSockets with JSON messages. Players log in with a device ID, add resources such as coins to their balance, and send resources to other players as gifts. Online recipients are notified instantly.

## Features

- **Device-ID login** — the first login registers a new player; each device can have one active connection at a time.
- **Resource balances** — players add `Coins` and `Rolls` to their balance.
- **Gifting** — players transfer resources to each other, and online recipients are notified in real time.
- **Attribute-based routing** — `[CommandHandler]` methods handle incoming messages on both server and client, and `[RequiresAuthentication]` guards commands that need a login.
- **Consistent balances** — optimistic concurrency with retries, transactional transfers, and a database constraint that prevents negative balances.
- **Structured logging** — Serilog writes to the console and to rolling daily log files, one per level.

## Tech stack

.NET 6 · ASP.NET Core (Kestrel) WebSockets · Entity Framework Core 6 · SQLite · Newtonsoft.Json · Serilog · Polly

## Project structure

All projects live in `src/Game` and belong to `Game.sln`.

| Project | Responsibility |
| --- | --- |
| `Game.Server` | ASP.NET Core host: accepts WebSocket connections, tracks sessions, handles commands |
| `Game.Client` | Interactive console client |
| `Game.Shared` | Message models, JSON message factory, WebSocket session and command router used by both apps |
| `Game.BusinessLogicLayer` | Player and gift services with validation rules |
| `Game.DataAccess` | EF Core `DbContext`, entities, migrations, repositories and unit of work |

## Getting started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0), or a newer .NET SDK together with the .NET 6 and ASP.NET Core 6 runtimes (all projects target `net6.0`).

### Build

```bash
git clone https://github.com/gmuradyants/game-websocket-server-and-client.git
cd game-websocket-server-and-client/src/Game
dotnet build
```

### Database

The repository ships with a ready-to-use SQLite database, `src/Game/Game.Server/Database/GameDatabase.db`, with all migrations applied and the `Coins` and `Rolls` resource types seeded. No setup is needed.

To reset it to a clean state, delete `GameDatabase.db` and recreate it from `src/Game`:

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 6.0.22
dotnet ef database update --project Game.DataAccess --startup-project Game.Server
```

The local tool manifest pins the EF Core CLI to 6.x to match the project's EF Core version (`dotnet-ef` 10.x, for example, cannot load `net6.0` projects). `Game.Server` is the startup project because it provides the connection string.

### Run

Start the server first, then one client per player, each in its own terminal:

```bash
# Terminal 1: the server, listening on ws://localhost:5003/ws
cd src/Game/Game.Server
dotnet run
```

```bash
# Terminal 2, 3, …: one client per player
cd src/Game/Game.Client
dotnet run
```

Each client lists the available commands and connects when you press **Enter**. If the server is unreachable, the client retries three times, two seconds apart, and then exits.

In Visual Studio, open `src/Game/Game.sln` and configure **Game.Server** and **Game.Client** as multiple startup projects.

## Usage

### Client commands

| Command | Example | Description |
| --- | --- | --- |
| `l <device_id>` | `l device-a` | Log in. The first login with a new device ID creates a player; the reply includes your player ID. |
| `ur <resource_type> <resource_value>` | `ur coins 100` | Add `<resource_value>` to your balance of the given resource. |
| `sg <friend_player_id> <resource_type> <resource_value>` | `sg 2 coins 30` | Send `<resource_value>` from your balance to another player. |

`ur` and `sg` require a login. The available resource types are `Coins` and `Rolls` (case-insensitive).

### Example session

Client 1:

```text
l device-a
[12:38:06 INF] You've successfully logged in. Your unique player ID is 1.
ur coins 100
[12:38:09 INF] Your 'coins' have been updated. New balance is: 100
sg 2 coins 30
```

Client 2:

```text
l device-b
[12:38:07 INF] You've successfully logged in. Your unique player ID is 2.
[12:38:14 INF] Gift received! Gift details: ResourceType - coins, Sender - 1, ResourceValue - 30
```

## WebSocket protocol

Any WebSocket client can talk to the server at `ws://localhost:5003/ws`. Each message is a JSON text frame whose `CommandType` field identifies it. The server sends `CommandType` as a number and accepts either the number or the name.

**Client → server**

| CommandType | Fields | Login required | Response |
| --- | --- | --- | --- |
| `Login` (1) | `DeviceId` | No | `UserLoggedIn` |
| `UpdateResource` (3) | `ResourceType`, `ResourceValue` | Yes | `BalanceUpdated` |
| `SendGift` (2) | `FriendPlayerId`, `ResourceType`, `ResourceValue` | Yes | None to the sender; `GiftReceived` to the recipient if online |

**Server → client**

| CommandType | Fields | Sent when |
| --- | --- | --- |
| `UserLoggedIn` (7) | `PlayerId` | Login succeeded |
| `BalanceUpdated` (4) | `NewBalance`, `ResourceType` | A resource update was applied |
| `GiftReceived` (5) | `FromPlayerId`, `ResourceType`, `ResourceValue` | Another player sent you a gift |
| `Notification` (6) | `Message` | Login required, already logged in, or the command has no handler |
| `Error` (0) | `Error` | Validation failed, for example an unknown player or resource type, or an insufficient balance |

Example exchange:

```text
→ {"CommandType":1,"DeviceId":"device-a"}
← {"CommandType":7,"PlayerId":1}
→ {"CommandType":3,"ResourceType":"Coins","ResourceValue":100}
← {"CommandType":4,"NewBalance":100.0,"ResourceType":"Coins"}
```

Each device ID can have one active connection. Logging in from a second connection closes the first with close status `1008` (policy violation).

## How it works

- **Server** — Kestrel listens on port 5003 and maps `/ws` to a middleware that accepts the WebSocket upgrade. `GameServer` keeps track of open sessions and the player each one is logged in as.
- **Sessions** — `WebSocketSession`, shared by server and client, runs a receive loop and a queued send loop backed by a TPL Dataflow `BufferBlock`.
- **Routing** — at startup, `WebSocketRouter` scans the entry assembly for handler hubs and their `[CommandHandler(CommandType.…)]` methods. Each message is dispatched to its handler in a fresh dependency-injection scope, and `[RequiresAuthentication]` rejects commands from connections that have not logged in.
- **Data** — services work through repositories and a unit of work. Balance updates use `ResourceValue` as an optimistic-concurrency token and retry on conflict, and a gift's debit, credit and gift record are committed in one transaction.

## Configuration

| Setting | Defined in | Value |
| --- | --- | --- |
| Server port and client URI | `Game.Shared/Constants.cs` | Port `5003` on all network interfaces; `ws://localhost:5003/ws` |
| Database connection string | `Game.Server/Startup.cs` | `Filename=Database/GameDatabase.db`, relative to the server's working directory |
| Log files | `Game.Shared/Logging/LoggerProvider.cs` | `logs/<level>/` in the app's output folder, for example `bin/Debug/net6.0/logs` |

Kestrel binds the port explicitly in `Game.Server/Program.cs`, so the URLs in `launchSettings.json` are not used.

## Troubleshooting

- **Logging in gets no reply, and the server logs `SQLite Error 14: 'unable to open database file'`.** The server was started from a folder other than `Game.Server`, so the relative database path doesn't resolve. Start it with `dotnet run` from `src/Game/Game.Server`.
- **`dotnet ef` fails with `Could not load file or assembly 'System.Runtime, Version=…'`.** The EF Core CLI is newer than the project's EF Core 6. Use `dotnet-ef` 6.x as shown in [Database](#database).
- **The client logs `Error connecting to the server` and exits.** The server is not running. Start the server first.

## License

Released under the [MIT License](LICENSE).
