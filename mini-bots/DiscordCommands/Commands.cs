using ExpressionTree;
using LuaManagement;
using Watson.ORM.Sqlite;

namespace MiniBots
{
    class Commands
    {
        public static void CreateBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            Console.WriteLine("Creating bot");

            string code = e.Message.Content.Substring(5).Trim();
            int codeStartIndex = code.IndexOf("```");

            string name = code.Substring(0, codeStartIndex).Trim();

            code = code.Substring(codeStartIndex); // Remove name from code
                                                   // Remove code block characters
            code = code.Replace("```lua", "");
            code = code.Replace("```", "");

            // Update existing bot if new bot with the same name is created
            bool miniBotExists = false;
            MiniBot? oldMiniBot = GetMiniBotByName(orm, name);

            if (oldMiniBot != null)
            {
                oldMiniBot.Code = code;
                orm.Update<MiniBot>(oldMiniBot);

                miniBotExists = true;
            }

            if (!miniBotExists)
            {
                MiniBot miniBot = new MiniBot { Name = name, Code = code, Storage = "" };
                orm.Insert<MiniBot>(miniBot);
            }
        }

        public static void Help(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            // Tell user how to use the bot, and limits of the bot
            String helpMessage = "Mini Bot Help\n" +
                "To create a bot, type: ```!bot <name> <3x:`>lua \n<code> \n<3x:`> ```\n" +
                "List bots: !list\n" +
                "Get bot code: !get <name>\n" +
                "View help: !help";

            SendDiscordMessage(helpMessage, e);
        }

        public static void ListBots(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            // List all bots

            String message = "Bots: \n";

            // Select all records
            List<MiniBot> miniBots = orm.SelectMany<MiniBot>();

            foreach (MiniBot miniBot in miniBots)
            {
                message += "- " + miniBot.Name + "\n";
            }

            SendDiscordMessage(message, e);
        }

        public static void GetBotCode(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            string name = e.Message.Content.Substring(5).Trim();

            MiniBot? miniBot = GetMiniBotByName(orm, name);

            if (miniBot != null)
            {
                SendDiscordMessage("```" + miniBot.Code + "```", e);
            }
        }

        public static void DeleteBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm)
        {
            string name = e.Message.Content.Substring(7).Trim();
            MiniBot? miniBot = GetMiniBotByName(orm, name);

            if (miniBot != null)
            {
                orm.Delete<MiniBot>(miniBot);
                SendDiscordMessage("Deleted: " + name, e);
            }
            else
            {
                SendDiscordMessage("No bot with name: " + name, e);
            }
        }

        public static void RunBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, WatsonORM orm, DiscordLua discordLua)
        {
            // Run all bots with message as input

            // Select all records
            List<MiniBot> miniBots = orm.SelectMany<MiniBot>();

            foreach (MiniBot miniBot in miniBots)
            {
                string botOutput = "";
                try
                {
                    botOutput = discordLua.Run(miniBot.Id, miniBot.Code, e.Message, orm);
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

        private static MiniBot? GetMiniBotByName(WatsonORM orm, string name)
        {
            Expr selectFilter = new Expr(
                orm.GetColumnName<MiniBot>(nameof(MiniBot.Name)),
                OperatorEnum.Equals,
                name);
            // Select all records
            List<MiniBot> miniBots = orm.SelectMany<MiniBot>(null, null, selectFilter);

            if (miniBots.Count > 1)
            {
                // This should never happen
                Console.WriteLine("More than 1 bot with name: " + name);
            }
            if (miniBots.Count <= 0)
            {
                return null;
            }

            return miniBots[0];
        }


        private static async void SendDiscordMessage(string message, DSharpPlus.EventArgs.MessageCreateEventArgs e)
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
}