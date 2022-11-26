using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramService
{

    private readonly IConfiguration _config;
    private readonly TwitchService _twitchProxy;

    private readonly long[] _allowedChats;
    private readonly string[] _bannedCommands;


    public TelegramService(IConfiguration config, TwitchService twitchService)
    {
        _config = config;
        _twitchProxy = twitchService;
        _allowedChats = _config["ALLOWED_CHATS"]!.Split(';').Select(x => long.Parse(x)).ToArray();
        _bannedCommands = _config["BANNED_COMMANDS"]!.Split(';');

    }

    public async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        var botClient = new TelegramBotClient(_config["TELEGRAM_TOKEN"]!);

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        //var me = await botClient.GetMeAsync();
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        if (!_allowedChats.Any(x => x == chatId))
            return;

        if (!messageText.StartsWith('$'))
            return;

        if (_bannedCommands.Any(x => x == messageText.Split(' ')[0].Replace("$", "")))
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "This command isn't supported in telegram proxy",
                cancellationToken: cancellationToken);
        }

        var response = await _twitchProxy.GetAnswerFromSupinic(messageText);

        await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: response.Replace("woahblanketbot,", $"@{message.From.Username}"),
                 cancellationToken: cancellationToken);
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}