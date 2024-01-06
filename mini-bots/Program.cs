using System.Text;
using DSharpPlus;
using NLua;
using Watson.ORM.Sqlite;
using Watson.ORM.Core;
using ExpressionTree;
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
            string token;
            DiscordLua discordLua = new DiscordLua();

            // Read environment variable if file doesn't exist
            if (!File.Exists("./token.txt"))
            {
                if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") == null)
                {
                    Console.WriteLine("DISCORD_TOKEN environment variable not set");
                    return;
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
                    return;
                }
            }

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });


            // Initialize database
            string databasePath = "./Database/mini-bots.db";
            DatabaseSettings settings = new DatabaseSettings(databasePath);
            WatsonORM orm = new WatsonORM(settings);
            orm.InitializeDatabase();
            orm.InitializeTable(typeof(MiniBot));

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

                    Console.WriteLine("Command: " + command);

                    switch (command)
                    {
                        case "help":
                            Help(e);
                            break;
                        case "list":
                            ListBots(e, orm);
                            break;
                        case "get":
                            GetBotCode(e, orm);
                            break;
                        case "delete":
                            DeleteBot(e, orm);
                            break;
                        case "bot":
                            CreateBot(e, orm);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    // Run all bots with message as input

                    // Select all records
                    List<MiniBot> miniBots = orm.SelectMany<MiniBot>();

                    foreach (MiniBot miniBot in miniBots)
                    {
                        string botOutput = "";
                        try
                        {
                            botOutput = discordLua.Run(miniBot.Id, miniBot.Code, e.Message, orm);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        if (botOutput != "")
                        {
                            SendDiscordMessage(botOutput, e);
                        }
                    }
                }

                return Task.CompletedTask;
            };


            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void CreateBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            Console.WriteLine("Creating bot");

            string code = e.Message.Content.Substring(5).Trim();
            int codeStartIndex = code.IndexOf("```");

            string name = code.Substring(0, codeStartIndex).Trim();

            code = code.Substring(codeStartIndex); // Remove name from code
                                                   // Remove code block characters
            code = code.Replace("```lua", "");
            code = code.Replace("```", "");

            // Update existing bot if new bot with the same name is created
            bool miniBotExists = false;
            MiniBot? oldMiniBot = GetMiniBotByName(orm, name);

            if (oldMiniBot != null)
            {
                oldMiniBot.Code = code;
                orm.Update<MiniBot>(oldMiniBot);

                miniBotExists = true;
            }

            if (!miniBotExists)
            {
                MiniBot miniBot = new MiniBot { Name = name, Code = code, Storage = "" };
                orm.Insert<MiniBot>(miniBot);
            }
        }

        private static void Help(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            // Tell user how to use the bot, and limits of the bot
            String helpMessage = "Mini Bot Help\n" +
                "To create a bot, type: ```!bot <name> <3x:`>lua \n<code> \n<3x:`> ```\n" +
                "List bots: !list\n" +
                "Get bot code: !get <name>\n" +
                "View help: !help";

            SendDiscordMessage(helpMessage, e);
        }

        private static void ListBots(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            // List all bots

            String message = "Bots: \n";

            // Select all records
            List<MiniBot> miniBots = orm.SelectMany<MiniBot>();

            foreach (MiniBot miniBot in miniBots)
            {
                message += "- " + miniBot.Name + "\n";
            }

            SendDiscordMessage(message, e);
        }

        private static void GetBotCode(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            string name = e.Message.Content.Substring(5).Trim();

            MiniBot? miniBot = GetMiniBotByName(orm, name);

            if (miniBot != null)
            {
                SendDiscordMessage("```" + miniBot.Code + "```", e);
            }
        }

        private static void DeleteBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            string name = e.Message.Content.Substring(7).Trim();
            MiniBot? miniBot = GetMiniBotByName(orm, name);

            if (miniBot != null)
            {
                orm.Delete<MiniBot>(miniBot);
                SendDiscordMessage("Deleted: " + name, e);
            }
            else
            {
                SendDiscordMessage("No bot with name: " + name, e);
            }
        }

        public static MiniBot? GetMiniBotByName(WatsonORM orm, string name)
        {
            Expr selectFilter = new Expr(
                orm.GetColumnName<MiniBot>(nameof(MiniBot.Name)),
                OperatorEnum.Equals,
                name);
            // Select all records
            List<MiniBot> miniBots = orm.SelectMany<MiniBot>(null, null, selectFilter);

            if (miniBots.Count > 1)
            {
                // This should never happen
                Console.WriteLine("More than 1 bot with name: " + name);
            }
            if (miniBots.Count <= 0)
            {
                return null;
            }

            return miniBots[0];
        }

        public static async void SendDiscordMessage(string message, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            try
            {
                await e.Message.RespondAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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