


using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

public class TwitchService
{

    const int MAX_TIMEOUT = 5000;

    private readonly IConfiguration _config;
    private readonly TwitchClient _chat;

    private bool _wantsMessage = false;
    private string? _responseFromSupibot = null;

    public TwitchService(IConfiguration config)
    {
        _config = config;
        _chat = new TwitchClient();
        _chat.Initialize(new ConnectionCredentials(_config["TWITCH_USERNAME"]!, _config["TWITCH_TOKEN"]!));

    }

    public async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        _chat.OnMessageReceived += OnMessageReceived;
        _chat.OnConnected += (sender, args) =>
        {
            _chat.JoinChannel(_config["TWITCH_PROXY_CHANNEL"]!);
        };

        _chat.Connect();
    }

    public async Task<string> GetAnswerFromSupinic(string requestMessage)
    {
        _wantsMessage = true;
        _responseFromSupibot = null;
        _chat.SendMessage(_config["TWITCH_PROXY_CHANNEL"]!, requestMessage);


        int attempt = 0;
        while(_responseFromSupibot== null) {
            
            await Task.Delay(100);
            
            attempt++;

            if(attempt * 100 >= MAX_TIMEOUT) 
                _responseFromSupibot = "Looks like supibot is down Bruhbruh";
        }
        
        _wantsMessage = false;  
        return _responseFromSupibot!;
    }

    private void OnMessageReceived(object? sender, OnMessageReceivedArgs args)
    {
        if (args.ChatMessage.UserId != "68136884")
            return;

        if (!_wantsMessage)
            return;

        _responseFromSupibot = args.ChatMessage.Message;
    }



}