using DSharpPlus.Entities;

namespace MiniBots
{
	public class CustomEmbedBuilder
	{
		private readonly DiscordEmbedBuilder _embedBuilder;

		public CustomEmbedBuilder(DiscordUser bot)
		{
			_embedBuilder = new DiscordEmbedBuilder();
			_embedBuilder.WithColor(DiscordColor.Aquamarine);
		}

		public void AddTitle(string title)
		{
			_embedBuilder.WithTitle(title);
		}

		public void AddDescription(string description, Boolean append = false)
		{
			if (append)
			{
				_embedBuilder.Description += description;
			}
			else
			{
				_embedBuilder.WithDescription(description);
			}
		}

		public DiscordEmbed Build()
		{
			return _embedBuilder.Build();
		}
	}
}