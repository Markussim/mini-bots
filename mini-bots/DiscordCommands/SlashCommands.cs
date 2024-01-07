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
            CustomEmbedBuilder embedBuilder = new CustomEmbedBuilder(ctx.Client.CurrentUser);
            embedBuilder.AddTitle("Available bots:");
            if (miniBots.Count > 0)
            {
                for (int i = 0; i < miniBots.Count; i++)
                {
                    MiniBot miniBot = miniBots[i];
                    embedBuilder.AddDescription($"`{i + 1}.` {miniBot.Name}\n", true);
                }
            }
            else
            {
                embedBuilder.AddDescription("No bots available");
            }

            // Send message
            await Reponse(ctx, embedBuilder.Build());
        }

        [SlashCommand("get", "Get code of existing MiniBot")]
        public async Task GetCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            CustomEmbedBuilder embedBuilder = new CustomEmbedBuilder(ctx.Client.CurrentUser);

            if (miniBot != null)
            {
                embedBuilder.AddTitle(miniBot.Name);
                embedBuilder.AddDescription($"```lua\n{miniBot.Code}\n```");
            }
            else
            {
                embedBuilder.AddTitle("No bot with name: " + name);
                embedBuilder.AddDescription("List bots: /list");
            }

            await Reponse(ctx, embedBuilder.Build());
        }

        [SlashCommand("help", "Shows all the available commands and how to use them.")]
        public async Task HelpCommand(InteractionContext ctx)
        {

            CustomEmbedBuilder embedBuilder = new CustomEmbedBuilder(ctx.Client.CurrentUser);
            embedBuilder.AddTitle("Available commands:");

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

        private static void AddCommand(CustomEmbedBuilder embedBuilder, string name, string description, string usage)
        {
            embedBuilder.AddDescription($"`{name}`: {description}\nUsage: ```{usage}```\n", true);
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

        [SlashCommand("delete", "Delete a Minibot")]
        public async Task DeleteCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            CustomEmbedBuilder embedBuilder = new CustomEmbedBuilder(ctx.Client.CurrentUser);

            if (miniBot != null)
            {
                _databaseManager.DeleteMiniBot(miniBot.Id);
                embedBuilder.AddTitle("Deleted bot: " + miniBot.Name);
            }
            else
            {
                embedBuilder.AddTitle("No bot with name: " + name);
                embedBuilder.AddDescription("List bots: /list");
            }

            await Reponse(ctx, embedBuilder.Build());
        }


        private static async Task Reponse(InteractionContext ctx, DiscordEmbed embed, Boolean senderOnly = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(senderOnly));
        }
    }
}