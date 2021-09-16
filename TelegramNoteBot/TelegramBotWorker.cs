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
        private readonly ITelegramBotClient _telegramBotClient;

    public TelegramBotWorker(Handlers.TelegramLogicHandlers handlers, IConfiguration configuration, ITelegramBotClient telegramBotClient)
        {
            _handlers = handlers;
            _configuration = configuration;
            _telegramBotClient = telegramBotClient;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramBotClient.StartReceiving(new DefaultUpdateHandler(_handlers.HandleUpdateAsync, _handlers.HandleErrorAsync),
                      stoppingToken);
        }
    }
}
