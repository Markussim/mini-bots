# MiniBots Discord Bot

MiniBots is a simple Discord bot written in C# utilizing the DSharpPlus library that allows users to create and run small Lua scripts (referred to as "MiniBots") within a Discord server.

## Features

- Create MiniBots by submitting Lua code in a Discord message.
- List all created MiniBots.
- Respond to messages by running the Lua code of each MiniBot.

## Prerequisites

Before you can use the MiniBots Discord Bot, make sure you have:

- [.NET Core SDK](https://dotnet.microsoft.com/download) installed
- A Discord Bot Token (you can obtain one [here](https://discord.com/developers/applications))

## Setup

1. Clone the repository:
   ```
   git clone <repository-url>
   ```
2. Navigate to the cloned directory.
3. Create a `token.txt` file and place your Discord Bot Token inside it.

## Usage

1. Compile and run the bot:
   ```
   dotnet run dotnet run --volume NAME_OF_VOLUME:/App/Database
   ```
2. Within Discord, you can use the following commands:

### Creating a MiniBot

Type the command ` !bot <name> \n```lua \n<code> \n```  ` to create a new MiniBot with the specified Lua `code`. Make sure to replace `<name>` and `<code>` with the actual bot name and Lua script.

````
!bot MyBot
```lua
if(message == "ping") then
  return "pong"
end
 ```
````

### Listing MiniBots

Type `!list` to list all existing MiniBots by their name.

```
!list
```

### Viewing Help

Type `!help` to get information on how to use the bot.

```
!help
```

## Discord Event Handling

The bot listens to the `MessageCreated` event on the Discord server. Upon receiving a message with the appropriate command prefix (`!bot`, `!help`, or `!list`), the bot will process the command accordingly. If the message does not contain one of these command prefixes, the bot will run all the stored MiniBots Lua code using the message as input.

## DiscordLua Class

The `DiscordLua` class is responsible for running Lua code. It utilizes the `NLua` package as the bridge between .NET and Lua, allowing execution of Lua scripts within the context of a message sent on Discord.

## Security Notice

Be aware that allowing users to run custom code on your server can be potentially dangerous. The current implementation does not include security measures against malicious code or to sandbox the Lua environment. You should introduce proper security features before using this bot in a live environment.

## Additional Information

- Error handling is included and outputs are sent to the console.
- Standard output from Lua is captured and sent as a message in Discord.
- The bot token is read from `token.txt` for security reasons, so as not to embed the token in the source code.

## Contributions

Contributions are welcome! If you wish to contribute, please submit a pull request or create an issue on the repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements

- Thanks to the creators of [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) for their Discord API wrapper for C#.
- The NLua team for providing a bridge between .NET and Lua with the [NLua library](http://nlua.org/).
