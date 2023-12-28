using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "MTE4OTk4MTYwNTQxMDg5Nzk1MA.Gvmv_q.QUcNequxF4lNCPlq07X54okIrLdNvoqaNl7F6Q",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}