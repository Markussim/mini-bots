using DSharpPlus;
using DSharpPlus.SlashCommands;
using LuaManagement;
using Microsoft.Extensions.Configuration;

namespace MiniBots
{
    class Program
    {
        public static string Prefix = "!";

        static async Task Main(string[] args)
        {
            DiscordSettings discordSettings = GetDiscordSettings();
            DiscordLua discordLua = new DiscordLua();

            var discord = GetDiscordClient(discordSettings.Token);
            var slash = discord.UseSlashCommands();
            slash.RegisterCommands<SlashCommands>(discordSettings.GuildID);

            // Create the database connection
            DatabaseManager databaseManager = new DatabaseManager();

            discord.MessageCreated += (s, e) =>
            {
                // If message is from bot, ignore
                if (e.Author.IsBot) return Task.CompletedTask;

                string discordMessage = e.Message.Content;

                if (discordMessage.StartsWith(Prefix))
                {
                    // Extract command
                    string command = discordMessage.Substring(Prefix.Length).Split(" ")[0];

                    if (command == "bot")
                    {
                        Commands.CreateBot(e, databaseManager);
                    }

                }
                else
                {
                    Commands.RunBot(e, databaseManager, discordLua);
                }

                return Task.CompletedTask;
            };


            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static DiscordSettings GetDiscordSettings()
        {
            IConfigurationSection? discordSettings = null;

            var parentpath = Directory.GetParent(Directory.GetCurrentDirectory());

            if (parentpath != null)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(parentpath.FullName)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                discordSettings = configuration.GetSection("DiscordSettings");
            }

            string? discordToken = null;
            ulong? discordGuildID = null;
            if (discordSettings != null)
            {
                discordToken = discordSettings["Token"];
                if (discordToken == null)
                {
                    discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                }

                string? discordGuildIDstring = discordSettings["GuildID"];
                if (discordGuildIDstring == null)
                {
                    discordGuildIDstring = Environment.GetEnvironmentVariable("DISCORD_GUILDID");
                }

                if (discordGuildIDstring != null)
                {
                    try
                    {
                        discordGuildID = Convert.ToUInt64(discordGuildIDstring);
                    }
                    catch
                    {
                        throw new Exception("DISCORD_GUILDID/DiscordSettings.GuildID failed to parse");
                    }
                }
            }

            if (discordToken == null)
            {
                throw new Exception("DISCORD_TOKEN environment variable not set, and DiscordSettings.Token not declared in appsettings.json");
            }

            return new DiscordSettings(discordToken, discordGuildID);
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

    public class DiscordSettings
    {
        public string Token { get; }

        public ulong? GuildID { get; }

        public DiscordSettings(string token, ulong? guildID)
        {
            Token = token;
            GuildID = guildID;
        }
    }
}