using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GuraDiscordBot.Core;

public class GuraBot
{
    private DiscordSocketClient socketClient;
    private CommandService commandService;
    private IServiceProvider serviceProvider;

    private readonly string accessToken;
    private readonly int permissionsInteger;

    public GuraBot(string accessToken, int permissionsInteger)
    {
        socketClient = null;
        commandService = null;
        serviceProvider = null;

        this.accessToken = accessToken;
        this.permissionsInteger = permissionsInteger;
    }

    public async Task RunAsync()
    {
        socketClient = new DiscordSocketClient();
        commandService = new CommandService();
        serviceProvider = ConfigureServiceProvider();

        socketClient.Log += SocketClient_Log;
        await RegisterCommandsAsync();
        await socketClient.LoginAsync(TokenType.Bot, accessToken);
        await socketClient.StartAsync();
        await Task.Delay(-1);
    }
    private Task SocketClient_Log(LogMessage logMessage)
    {
        Console.WriteLine(logMessage);
        return Task.CompletedTask;
    }
    private async Task RegisterCommandsAsync()
    {
        socketClient.MessageReceived += HandleCommandAsync;
        Assembly assembly = Assembly.GetExecutingAssembly();
        await commandService.AddModulesAsync(assembly, serviceProvider);
    }
    private async Task HandleCommandAsync(SocketMessage socketMessage)
    {
        var message = socketMessage as SocketUserMessage;
        var context = new SocketCommandContext(socketClient, message);
        if (message.Author.IsBot) return;

        int argPos = 0;
        if (message.HasStringPrefix("!", ref argPos))
        {
            var result = await commandService.ExecuteAsync(context, argPos, serviceProvider);
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.ErrorReason);
            }
            if (result.Error.Equals(CommandError.UnmetPrecondition))
            {
                await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
    private IServiceProvider ConfigureServiceProvider()
    {
        var services = new ServiceCollection()
            .AddSingleton(socketClient)
            .AddSingleton(commandService);

        return services.BuildServiceProvider();
    }
}