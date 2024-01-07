using System.Net.NetworkInformation;
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

        private struct Command
        {
            public string Name;
            public string Description;
            public string Usage;
        }

        [SlashCommand("help", "Shows all the available commands and how to use them.")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            Command[] commands =
            [
                new Command
                {
                    Name = Program.Prefix + "bot",
                    Description = "Create a new MiniBot",
                    Usage = $"{Program.Prefix}bot <BotName>\n<3x`>\n<BotCode>\n<3x`>"
                },
                new Command
                {
                    Name = "/list",
                    Description = "List existing MiniBots",
                    Usage = "/list"
                },
                new Command
                {
                    Name = "/get",
                    Description = "Get code of existing MiniBot",
                    Usage = "/get <BotName>"
                },
                new Command
                {
                    Name = "/delete",
                    Description = "Delete a MiniBot",
                    Usage = "/delete <BotName>"
                },
                new Command
                {
                    Name = "/help",
                    Description = "Shows all the available commands and how to use them.",
                    Usage = "/help"
                }
            ];

            CustomEmbedBuilder embedBuilder = new CustomEmbedBuilder(ctx.Client.CurrentUser);
            embedBuilder.AddTitle("Available commands:");

            for (int i = 0; i < commands.Length; i++)
            {
                Command command = commands[i];
                embedBuilder.AddDescription($"`{command.Name}`: {command.Description}\nUsage: ```{command.Usage}```\n", true);
            }

            await Reponse(ctx, embedBuilder.Build(), true);
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