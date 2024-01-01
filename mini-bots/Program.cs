using System.Text;
using DSharpPlus;
using NLua;
using Microsoft.Data.Sqlite;

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

            String databasePath = "./Database/mini-bots.db";

            // Open database
            var connection = new SqliteConnection($"Data Source={databasePath}");

            connection.Open();

            // Create table if it doesn't exist
            var commandCreateTable = connection.CreateCommand();
            commandCreateTable.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS miniBots (
                    name TEXT NOT NULL,
                    code TEXT NOT NULL
                );
            ";
            commandCreateTable.ExecuteNonQuery();

            // Add bot-add command
            var commandAddBot = connection.CreateCommand();
            commandAddBot.CommandText =
            @"
                INSERT INTO miniBots (name, code)
                VALUES ($name, $code);
            ";

            // Add bot-replace command
            var commandRelaceBot = connection.CreateCommand();
            commandRelaceBot.CommandText =
            @"
                UPDATE miniBots
                SET code = $code
                WHERE name = $name; 
            ";

            // Add bots-get command
            var commandGetAllBots = connection.CreateCommand();
            commandGetAllBots.CommandText =
            @"
                SELECT name, code
                FROM miniBots;
            ";

            // Add bot-get command
            var commandGetBot = connection.CreateCommand();
            commandGetBot.CommandText =
            @"
                SELECT code
                FROM miniBots
                WHERE name = $name; 
            ";

            var commandDeleteBot = connection.CreateCommand();
            commandDeleteBot.CommandText =
            @"
                DELETE
                FROM miniBots
                WHERE name = $name;
            ";

            discord.MessageCreated += (s, e) =>
            {
                // If message is from bot, ignore
                if (e.Author.IsBot) return Task.CompletedTask;

                string discordMessage = e.Message.Content;


                if (discordMessage.StartsWith("!bot"))
                {
                    string code = discordMessage.Substring(5).Trim();
                    int codeStartIndex = code.IndexOf("```");

                    string name = code.Substring(0, codeStartIndex).Trim();

                    code = code.Substring(codeStartIndex); // Remove name from code
                                                           // Remove code block characters
                    code = code.Replace("```lua", "");
                    code = code.Replace("```", "");

                    // Update existing bot if new bot with the same name is created
                    bool miniBotExists = false;
                    using (var reader = commandGetAllBots.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string old_name = reader.GetString(0);
                            if (old_name == name)
                            {
                                commandRelaceBot.Parameters.Clear();
                                commandRelaceBot.Parameters.AddWithValue("$name", name);
                                commandRelaceBot.Parameters.AddWithValue("$code", code);
                                commandRelaceBot.ExecuteNonQuery();
                                miniBotExists = true;
                                break;
                            }
                        }
                    }

                    if (!miniBotExists)
                    {
                        // Add bot to list
                        commandAddBot.Parameters.Clear();
                        commandAddBot.Parameters.AddWithValue("$name", name);
                        commandAddBot.Parameters.AddWithValue("$code", code);
                        commandAddBot.ExecuteNonQuery();
                    }
                }
                else if (discordMessage.StartsWith("!help"))
                {
                    // Tell user how to use the bot, and limits of the bot
                    String helpMessage = "Mini Bot Help\n" +
                        "To create a bot, type: ```!bot <name> <3x:`>lua \n<code> \n<3x:`> ```\n" +
                        "List bots: !list\n" +
                        "Get bot code: !get <name>\n" +
                        "View help: !help";

                    SendDiscordMessage(helpMessage, e);

                }
                else if (discordMessage.StartsWith("!list"))
                {
                    // List all bots
                    String message = "Bots: \n";
                    using (var reader = commandGetAllBots.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            message += "- " + reader.GetString(0) + "\n";
                        }
                    }

                    SendDiscordMessage(message, e);
                }
                else if (discordMessage.StartsWith("!get"))
                {
                    string name = discordMessage.Substring(5).Trim();
                    commandGetBot.Parameters.Clear();
                    commandGetBot.Parameters.AddWithValue("$name", name);
                    using (var reader = commandGetBot.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SendDiscordMessage("```" + reader.GetString(0) + "```", e);
                        }
                    }
                }
                else if (discordMessage.StartsWith("!delete"))
                {
                    string name = discordMessage.Substring(7).Trim();
                    commandDeleteBot.Parameters.Clear();
                    commandDeleteBot.Parameters.AddWithValue("$name", name);
                    commandDeleteBot.ExecuteNonQuery();
                    SendDiscordMessage("Deleted: " + name, e);
                }
                else
                {
                    // Run all bots with message as input

                    using (var reader = commandGetAllBots.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string botOutput = "";
                            try
                            {
                                botOutput = discordLua.Run(reader.GetString(1), discordMessage);
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
                }

                return Task.CompletedTask;
            };


            await discord.ConnectAsync();
            await Task.Delay(-1);
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
        private Lua lua;

        public DiscordLua()
        {
            lua = new Lua();
        }

        public string Run(string code, string message = "")
        {
            Byte[] luaIn = Encoding.UTF8.GetBytes($"message = \"{message}\"\n" + code);
            // TODO: Escape message
            // TODO: Handle utf8 output
            object[] luaOutput = lua.DoString(luaIn);

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