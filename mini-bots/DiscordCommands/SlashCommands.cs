using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MiniBots
{
    public class SlashCommands : ApplicationCommandModule
    {
        readonly DatabaseManager _databaseManager = new DatabaseManager();

        [SlashCommand("list", "List existing MiniBots")]
        public async Task ListCommand(InteractionContext ctx)
        {
            // Get all bots
            List<MiniBot> miniBots = _databaseManager.GetMiniBots();

            // Create message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithTitle("Available bots:");
            if (miniBots.Count > 0)
            {
                for (int i = 0; i < miniBots.Count; i++)
                {
                    MiniBot miniBot = miniBots[i];
                    embedBuilder.Description += $"`{i + 1}.` {miniBot.Name} {GetEmoji(miniBot.Enabled)}\n";
                }
            }
            else
            {
                embedBuilder.WithDescription("No bots available");
            }

            // Send message
            await Reponse(ctx, embedBuilder.Build());
        }

        [SlashCommand("get", "Get code of existing MiniBot")]
        public async Task GetCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            if (miniBot != null)
            {
                embedBuilder.WithTitle($"{miniBot.Name} {GetEmoji(miniBot.Enabled)}");
                embedBuilder.WithDescription($"```lua\n{miniBot.Code}\n```");
            }
            else
            {
                embedBuilder.WithTitle("No bot with name: " + name);
                embedBuilder.WithDescription("List bots: /list");
            }

            await Reponse(ctx, embedBuilder.Build());
        }

        private string GetEmoji(bool input){
            return input ? "✅" : "❌";
        }

        [SlashCommand("help", "Shows all the available commands and how to use them.")]
        public async Task HelpCommand(InteractionContext ctx)
        {

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithTitle("Available commands:");

            foreach (Commands.LegacyCommand command in Commands.LegacyCommands)
            {
                AddCommand(embedBuilder, command.Name, command.Description, command.Usage);
            }

            MethodInfo[] methods = typeof(SlashCommands).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var slashCommandAttribute = method.GetCustomAttribute<SlashCommandAttribute>();
                if (slashCommandAttribute != null)
                {
                    string commandName = "/" + slashCommandAttribute.Name ?? method.Name.ToLower();
                    string commandDescription = slashCommandAttribute.Description ?? "No description available";
                    string commandUsage = GetCommandUsage(commandName, method);

                    AddCommand(embedBuilder, commandName, commandDescription, commandUsage);
                }
            }

            await Reponse(ctx, embedBuilder.Build(), true);
        }

        private static void AddCommand(DiscordEmbedBuilder embedBuilder, string name, string description, string usage)
        {
            embedBuilder.Description += $"`{name}`: {description}\nUsage: ```{usage}```\n";
        }
        private static string GetCommandUsage(string commandName, MethodInfo method)
        {
            var parameters = method.GetParameters();

            StringBuilder usageBuilder = new StringBuilder($"{commandName} ");
            foreach (var parameter in parameters)
            {
                OptionAttribute? optionAttribute = parameter.GetCustomAttribute<OptionAttribute>();
                string? paramName = optionAttribute?.Name;

                if (paramName != null)
                {
                    usageBuilder.Append($"<{paramName}> ");
                }
            }

            return usageBuilder.ToString().Trim();
        }

        [SlashCommand("disable", "Disable a Minibot without deleting it.")]
        public async Task DisableCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            await SetEnabled(ctx, name, false);
        }

        [SlashCommand("enable", "Enable a Minibot.")]
        public async Task EnableCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            await SetEnabled(ctx, name, true);
        }

        private async Task SetEnabled(InteractionContext ctx, string name, bool enable){
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            if (miniBot != null)
            {
                _databaseManager.SetMiniBotEnabled(miniBot.Id, enable);
                string titleText = enable ? "Enabled" : "Disabled";
                embedBuilder.WithTitle($"{titleText} bot: " + miniBot.Name);
            }
            else
            {
                embedBuilder.WithTitle("No bot with name: " + name);
                embedBuilder.WithDescription("List bots: /list");
            }

            await Reponse(ctx, embedBuilder.Build());
        }

        [SlashCommand("delete", "Delete a Minibot")]
        public async Task DeleteCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            if (miniBot != null)
            {
                _databaseManager.DeleteMiniBot(miniBot.Id);
                embedBuilder.WithTitle("Deleted bot: " + miniBot.Name);
            }
            else
            {
                embedBuilder.WithTitle("No bot with name: " + name);
                embedBuilder.WithDescription("List bots: /list");
            }

            await Reponse(ctx, embedBuilder.Build());
        }


        private static async Task Reponse(InteractionContext ctx, DiscordEmbed embed, bool senderOnly = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(senderOnly));
        }
    }
}