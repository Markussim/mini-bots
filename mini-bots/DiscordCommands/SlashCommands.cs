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
            String message = "Bots: \n";

            // Get all bots
            List<MiniBot> miniBots = _databaseManager.GetMiniBots();

            // Add string builder
            StringBuilder sb = new StringBuilder();


            foreach (MiniBot miniBot in miniBots)
            {
                //message += "- " + miniBot.Name + "\n";
                sb.Append("- " + miniBot.Name + "\n");
            }

            message += sb.ToString();

            await Reponse(ctx, message);
        }

        [SlashCommand("get", "Get code of existing MiniBot")]
        public async Task GetCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            if (miniBot != null)
            {
                await Reponse(ctx, "```lua" + miniBot.Code + "```");
            }
            else
            {
                await Reponse(ctx, "no such bot you fool");
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

            await Reponse(ctx, helpMessage);
        }

        [SlashCommand("delete", "Delete a Minibot")]
        public async Task DeleteCommand(InteractionContext ctx, [Option("BotName", "who will it be")] string name)
        {
            MiniBot? miniBot = _databaseManager.GetMiniBotByName(name);

            if (miniBot != null)
            {
                _databaseManager.DeleteMiniBot(miniBot.Id);
                await Reponse(ctx, "Deleted: " + name);
            }
            else
            {
                await Reponse(ctx, "No bot with name: " + name);
            }
        }



        private static async Task Reponse(InteractionContext ctx, string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message));
        }
    }
}