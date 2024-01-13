# MiniBots Discord Bot

MiniBots is a Discord bot written in C# utilizing the DSharpPlus library that allows users to create and run small Lua scripts (referred to as "MiniBots") within a Discord server.

## Prerequisites

Before you can use the MiniBots Discord Bot, make sure you have:

- [.NET Core SDK](https://dotnet.microsoft.com/download) installed
- A Discord Bot Token (you can obtain one [here](https://discord.com/developers/applications))

## Setup

### Running manually

1. Clone the repository:
   ```
   git clone https://github.com/Markussim/mini-bots.git
   ```
2. Navigate to the cloned directory.
3. Copy `appsettings.json.example` to `appsettings.json` and enter your Discord token.
4. Compile and run the bot:
   ```
   dotnet run
   ```

### Running in Docker

1. Clone the docker image:
   ```
   docker pull ghcr.io/markussim/mini-bots:latest
   ```
2. Start the docker container:
   ```
   docker run -d -e DISCORD_TOKEN=<discord token> -v ./database:/App/Database mini-bots:latest
   ```
   Optional Environments Variables:
   - DISCORD_GUILDID
   - DISCORD_PREFIX

## Usage

### Creating a MiniBot

Type the command ` ?bot <name> \n```lua \n<code> \n```  ` to create a new MiniBot with the specified Lua `code`. Make sure to replace `<name>` and `<code>` with the actual bot name and Lua script.

Example:

````
?bot Ping_Pong
```lua
if string.lower(messageManager.Content) == "ping" then
  return "pong"
end
```
````

More examples can be found in the [examples folder](/Examples/).

### Viewing Help

Use the `/help` command to get information on how to use the bot.

## Security Notice

Be aware that allowing users to run custom code on your server can be potentially dangerous. The current implementation does not include security measures against malicious code or to sandbox the Lua environment. You should introduce proper security features before using this bot in a live environment.

## Contributions

Contributions are welcome! If you wish to contribute, please submit a pull request or create an issue on the repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements

- Thanks to the creators of [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) for their Discord API wrapper for C#.
- The NLua team for providing a bridge between .NET and Lua with the [NLua library](http://nlua.org/).
