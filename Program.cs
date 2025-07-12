using Discord;
using Discord.WebSocket;
using Discord.Net;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;

class Program
{
    private static DiscordSocketClient _client;

    class Config
    {
        public string Token { get; set; }
    }

    static async Task Main(string[] args)
    {
        var config = LoadConfig();
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.SlashCommandExecuted += SlashCommandHandler;

        await _client.LoginAsync(TokenType.Bot, config.Token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Config LoadConfig()
    {
        string json = File.ReadAllText("config.json");
        return JsonConvert.DeserializeObject<Config>(json);
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static async Task ReadyAsync()
    {
        Console.WriteLine($"✅ Bot connected as {_client.CurrentUser.Username}");

        var command = new SlashCommandBuilder()
            .WithName("message")
            .WithDescription("Send a customized embed message")
            .AddOption("title", ApplicationCommandOptionType.String, "The title of the embed", isRequired: true)
            .AddOption("description", ApplicationCommandOptionType.String, "The main content", isRequired: true)
            .AddOption("color", ApplicationCommandOptionType.String, "Hex color like #FF0000", isRequired: false)
            .AddOption("image", ApplicationCommandOptionType.String, "Image URL", isRequired: false)
            .AddOption("thumbnail", ApplicationCommandOptionType.String, "Thumbnail URL", isRequired: false);

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(command.Build());
            Console.WriteLine("🌐 Slash command registered.");
        }
        catch (HttpException ex)
        {
            Console.WriteLine($"❌ Command error: {ex.Message}");
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if (command.Data.Name != "message") return;

        var title = command.Data.Options.FirstOrDefault(x => x.Name == "title")?.Value?.ToString();
        var description = command.Data.Options.FirstOrDefault(x => x.Name == "description")?.Value?.ToString();
        var colorHex = command.Data.Options.FirstOrDefault(x => x.Name == "color")?.Value?.ToString() ?? "#0099ff";
        var image = command.Data.Options.FirstOrDefault(x => x.Name == "image")?.Value?.ToString();
        var thumbnail = command.Data.Options.FirstOrDefault(x => x.Name == "thumbnail")?.Value?.ToString();

        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(new Color(uint.Parse(colorHex.Replace("#", ""), NumberStyles.HexNumber)));

        if (!string.IsNullOrWhiteSpace(image)) embed.WithImageUrl(image);
        if (!string.IsNullOrWhiteSpace(thumbnail)) embed.WithThumbnailUrl(thumbnail);

        await command.RespondAsync(embed: embed.Build(), ephemeral: true); // optional: hidden from others
    }
}
