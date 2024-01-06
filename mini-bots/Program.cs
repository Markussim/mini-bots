using DSharpPlus;
using Watson.ORM.Sqlite;
using DatabaseWrapper.Core;

namespace MiniBots
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string token = GetToken();
            DiscordLua discordLua = new DiscordLua();

            var discord = GetDiscordClient(token);

            WatsonORM orm = GetORM();

            discord.MessageCreated += (s, e) =>
            {
                // If message is from bot, ignore
                if (e.Author.IsBot) return Task.CompletedTask;

                string discordMessage = e.Message.Content;

                string prefix = "!";

                if (discordMessage.StartsWith(prefix))
                {
                    // Extract command
                    string command = discordMessage.Substring(prefix.Length).Split(" ")[0];

                    switch (command)
                    {
                        case "help":
                            Commands.Help(e);
                            break;
                        case "list":
                            Commands.ListBots(e, orm);
                            break;
                        case "get":
                            Commands.GetBotCode(e, orm);
                            break;
                        case "delete":
                            Commands.DeleteBot(e, orm);
                            break;
                        case "bot":
                            Commands.CreateBot(e, orm);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    Commands.RunBot(e, orm, discordLua);
                }

                return Task.CompletedTask;
            };


            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static string GetToken()
        {
            string token;
            // Read environment variable if file doesn't exist
            if (!File.Exists("./token.txt"))
            {
                string? envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

                if (envToken == null)
                {
                    throw new Exception("DISCORD_TOKEN environment variable not set");
                }
                else
                {
                    return envToken;
                }
            }
            else
            {
                // Read the token from a file instead of having it in code
                try
                {
                    token = File.ReadAllText("./token.txt").Trim(); // Make sure to update the path to the actual file location
                    return token;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not read the token file: {ex.Message}");
                    throw;
                }
            }
        }

        private static WatsonORM GetORM()
        {
            string databasePath = "./Database/mini-bots.db";
            DatabaseSettings settings = new DatabaseSettings(databasePath);
            WatsonORM orm = new WatsonORM(settings);
            orm.InitializeDatabase();
            orm.InitializeTable(typeof(MiniBot));

            return orm;
        }

        private static DiscordClient GetDiscordClient(string token)
        {
            DiscordClient discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            return discord;
        }

    }
}