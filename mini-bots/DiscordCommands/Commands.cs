using LuaManagement;

namespace MiniBots
{
    class Commands
    {
        public struct LegacyCommand
        {
            public string Name;
            public string Description;
            public string Usage;
        }

        public static LegacyCommand[] LegacyCommands = [
            new LegacyCommand
            {
                Name = Program.Prefix + "bot",
                Description = "Create a new bot",
                Usage =  Program.Prefix + "bot <name>\n<3x`>lua\n\t<code>\n<3x`>"
            },
        ];

        public static void CreateBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, DatabaseManager databaseManager)
        {
            Console.WriteLine("Creating bot");

            string code = e.Message.Content.Substring(5).Trim();
            int codeStartIndex = code.IndexOf("```");

            string name = code.Substring(0, codeStartIndex).Trim();

            code = code.Substring(codeStartIndex); // Remove name from code
                                                   // Remove code block characters
            code = code.Replace("```lua", "");
            code = code.Replace("```", "");

            databaseManager.CreateMiniBotIfNotExists(name, code);

        }

        public static void RunBot(DSharpPlus.EventArgs.MessageCreateEventArgs e, DatabaseManager databaseManager, DiscordLua discordLua)
        {
            // Run all bots with message as input

            // Select all records
            List<MiniBot> miniBots = databaseManager.GetMiniBots();

            foreach (MiniBot miniBot in miniBots)
            {
                string botOutput = "";
                try
                {
                    botOutput = discordLua.Run(miniBot.Id, miniBot.Code, e.Message, databaseManager);
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