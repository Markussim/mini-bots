using System;
using System.IO; // Need to add this
using DSharpPlus;
using DSharpPlus.Entities;
// using DSharpPlus.SlashCommands;
using NLua;

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string token;
            DiscordLua discordLua = new DiscordLua();

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
            //var slash = discord.UseSlashCommands();
            //slash.RegisterCommands<SlashCommands>(554977304665784325);

            discord.MessageCreated += async (s, e) =>
            {
                // If message is from bot, ignore
                if (e.Author.IsBot) return;

                string discordMessage = e.Message.Content;


                Console.WriteLine(discordMessage);
                if (discordMessage.StartsWith("!bot"))
                {
                    string code = discordMessage.Substring(5).Trim();
                    code = code.Replace("```lua", "");
                    code = code.Replace("```", "");
                    Console.WriteLine("Running lua: " + code);
                    try
                    {
                        await e.Message.RespondAsync(discordLua.Run(code));
                    }
                    catch (Exception ex)
                    {
                        await e.Message.RespondAsync(ex.Message);
                    }
                }
                else
                {
                    await e.Message.RespondAsync("Pucko");
                }

            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
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
            object[] luaOutput = lua.DoString(code);

            if (luaOutput.Length > 0)
            {
                var stringOut = luaOutput[0].ToString();
                if (stringOut != null)
                {
                    return stringOut;
                }
                else
                {
                    return "No output";
                }
            }
            else
            {
                return "No output";
            }

        }
    }
}