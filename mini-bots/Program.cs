using System.Text;
using DSharpPlus;
using NLua;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using DatabaseWrapper.Core;
using DSharpPlus.Entities;

// Apply attributes to your class
[Table("miniBots")]
public class MiniBot
{
    [Column("id", true, DataTypes.Int, false)]
    public int Id { get; set; }

    [Column("name", false, DataTypes.Nvarchar, 64, false)]
    public string Name { get; set; }

    [Column("code", false, DataTypes.Nvarchar, 64, false)]
    public string Code { get; set; }

    [Column("storage", false, DataTypes.Nvarchar, 64, false)]
    public string Storage { get; set; }

    // Parameter-less constructor is required
    public MiniBot()
    {
    }
}

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
                if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") == null)
                {
                    Console.WriteLine("DISCORD_TOKEN environment variable not set");
                    return null;
                }
                token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            }
            else
            {
                // Read the token from a file instead of having it in code
                try
                {
                    token = File.ReadAllText("./token.txt").Trim(); // Make sure to update the path to the actual file location
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not read the token file: {ex.Message}");
                    return null;
                }
            }

            if (token == null)
            {
                throw new Exception("Token is null");
            }

            return token;
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
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            return discord;
        }

    }

    // Lua class
    public class DiscordLua
    {
        private Lua _lua;

        public DiscordLua()
        {
            _lua = new Lua();
        }

        public string Run(int id, string code, DiscordMessage message, WatsonORM orm)
        {
            _lua["messageManager"] = new MessageManager(message);
            _lua["timeManager"] = new TimeManager();
            _lua["storageManager"] = new StorageManager(orm, id);

            Byte[] luaIn = Encoding.UTF8.GetBytes(code);

            // TODO: Handle utf8 output
            object[] luaOutput = _lua.DoString(luaIn);

            if (luaOutput.Length > 0)
            {
                var stringOut = luaOutput[0].ToString();
                if (stringOut != null)
                {
                    return stringOut;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

        }
    }
}