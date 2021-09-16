using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace TelegramNoteBot
{
    internal class TelegramBotWorker : BackgroundService
    {
        private readonly Handlers.TelegramLogicHandlers _handlers;
        private readonly IConfiguration _configuration;
        private readonly string BotToken;

    public TelegramBotWorker(Handlers.TelegramLogicHandlers handlers, IConfiguration configuration)
        {
            _handlers = handlers;
            BotToken = _configuration.GetValue<string>("BotToken");
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TelegramBotClient _client = new TelegramBotClient(BotToken);
                _client.StartReceiving(new DefaultUpdateHandler(_handlers.HandleUpdateAsync, _handlers.HandleErrorAsync),
                   stoppingToken);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
