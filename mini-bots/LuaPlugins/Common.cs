using DSharpPlus.Entities;

namespace MiniBots
{
    public class MessageManager
    {
        public string Content { get; }
        public string AuthorId { get; }
        public string AuthorUsername { get; }
        public string Channel { get; }


        public MessageManager(DiscordMessage discordMessage)
        {
            Content = discordMessage.Content;
            AuthorId = discordMessage.Author.Id.ToString();
            AuthorUsername = discordMessage.Author.Username;
            Channel = discordMessage.Channel.Name;
        }
    }

    public class TimeManager
    {
        public DateTime LocalNow()
        {
            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public int UnixNow()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public string Weekday()
        {
            return DateTime.Now.DayOfWeek.ToString();
        }

        public string Month()
        {
            return DateTime.Now.Month.ToString();
        }

        public string Year()
        {
            return DateTime.Now.Year.ToString();
        }

        public string Day()
        {
            return DateTime.Now.Day.ToString();
        }

        public string Hour()
        {
            return DateTime.Now.Hour.ToString();
        }

        public string Minute()
        {
            return DateTime.Now.Minute.ToString();
        }
    }
}