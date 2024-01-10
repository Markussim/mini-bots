using DSharpPlus;
using DSharpPlus.SlashCommands;
using LuaManagement;
using Microsoft.Extensions.Configuration;

namespace MiniBots
{
    class Program
    {
        public static string Prefix = "";

        static async Task Main(string[] args)
        {
            DiscordSettings discordSettings = GetDiscordSettings();
            Prefix = discordSettings.CommandPrefix;

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

            string? discordToken = GetDiscordSetting(discordSettings, "Token", "DISCORD_TOKEN", true);
            if (discordToken == null) throw new Exception("This should never happen");

            string? commandPrefix = GetDiscordSetting(discordSettings, "Prefix", "DISCORD_PREFIX", false);

            string? discordGuildIDstring = GetDiscordSetting(discordSettings, "GuildID", "DISCORD_GUILDID", false);

            ulong? discordGuildID = null;
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

            return new DiscordSettings(discordToken, discordGuildID, commandPrefix);
        }

        private static string? GetDiscordSetting(IConfigurationSection? discordSettings, string appsettingsName, string environmentName, bool required)
        {
            string? setting = null;

            if (discordSettings != null)
            {
                setting = discordSettings[appsettingsName];

                if (setting == null)
                {
                    setting = Environment.GetEnvironmentVariable(environmentName);
                }
            }

            if (setting == null && required)
            {
                throw new Exception(environmentName + " environment variable not set, and DiscordSettings." + appsettingsName + " not declared in appsettings.json");
            }

            return setting;
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

        public string CommandPrefix { get; } = "?";

        public DiscordSettings(string token, ulong? guildID, string? commandPrefix)
        {
            Token = token;
            GuildID = guildID;

            if (commandPrefix != null)
            {
                CommandPrefix = commandPrefix;
            }
        }
    }
}