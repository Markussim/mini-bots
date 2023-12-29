using System;
using System.IO; // Need to add this
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using NLua;
using Microsoft.Data.Sqlite;

namespace MiniBots
{
    class Program
    {
        struct MiniBot
        {
            public string name;
            public string code;
        }

        static async Task Main(string[] args)
        {
            string token;
            DiscordLua discordLua = new DiscordLua();

            List<MiniBot> miniBots = new List<MiniBot>();

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

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            // Open database
            var connection = new SqliteConnection("Data Source=mini-bots.db");

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

            var commandAddBot = connection.CreateCommand();
            commandAddBot.CommandText =
            @"
                    INSERT INTO miniBots (name, code)
                    VALUES ($name, $code);
                ";
            commandAddBot.Parameters.AddWithValue("$name", "testname");
            commandAddBot.Parameters.AddWithValue("$code", "testcode");
            commandAddBot.ExecuteNonQuery();

            var commandGetAllBots = connection.CreateCommand();
            commandGetAllBots.CommandText =
            @"
                    SELECT name, code
                    FROM miniBots;
                ";

            // Test reading from database
            using (var reader = commandGetAllBots.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}");
                }
            }

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
                    for (int i = 0; i < miniBots.Count; i++)
                    {
                        if (miniBots[i].name == name)
                        {
                            miniBots[i] = new MiniBot { name = name, code = code };
                            miniBotExists = true;
                            break;
                        }
                    }
                    if (!miniBotExists)
                    {
                        // Add bot to list
                        miniBots.Add(new MiniBot { name = name, code = code });
                    }
                }
                else if (discordMessage.StartsWith("!help"))
                {
                    // Tell user how to use the bot, and limits of the bot
                    String helpMessage = "Mini Bot Help\n" +
                        "To create a bot, type: ```!bot <name> <3x:`>lua \n<code> \n<3x:`> ```\n" +
                        "List bots: !list\n" +
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
                    // if (miniBots.Count > 0)
                    // {
                    //     foreach (MiniBot miniBot in miniBots)
                    //     {
                    //         message += "- " + miniBot.name + "\n";
                    //     }
                    // }
                    // else
                    // {
                    //     message = "No bots running";
                    // }

                    SendDiscordMessage(message, e);
                }
                else
                {
                    // Run all bots with message as input
                    foreach (MiniBot miniBot in miniBots)
                    {
                        string botOutput = "";
                        try
                        {
                            botOutput = discordLua.Run(miniBot.code, discordMessage);
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