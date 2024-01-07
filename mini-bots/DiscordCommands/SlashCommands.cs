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

            if (miniBot != null)
            {
                await ReponseString(ctx, "```lua" + miniBot.Code + "```");
            }
            else
            {
                await ReponseString(ctx, "no such bot you fool");
            }
        }

        [SlashCommand("help", "Shows all the available commands and how to use them.")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            // Tell user how to use the bot, and limits of the bot
            string helpMessage = "Mini Bot Help\n" +
                "To create a bot, type: ```!bot <name> <3x:`>lua \n<code> \n<3x:`> ```\n" +
                "List bots: !list\n" +
                "Get bot code: !get <name>\n" +
                "View help: !help";

            await ReponseString(ctx, helpMessage);
        }

        [SlashCommand("delete", "Delete a Minibot")]
        public async Task DeleteCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            if (miniBot != null)
            {
                _databaseManager.DeleteMiniBot(miniBot.Id);
                await ReponseString(ctx, "Deleted: " + name);
            }
            else
            {
                await ReponseString(ctx, "No bot with name: " + name);
            }
        }



        private static async Task ReponseString(InteractionContext ctx, string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message));
        }

        private static async Task Reponse(InteractionContext ctx, DiscordEmbed embed)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}