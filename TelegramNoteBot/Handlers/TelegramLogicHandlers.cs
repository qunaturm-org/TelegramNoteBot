using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramNoteBot.Handlers
{
    public class TelegramLogicHandlers
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageProcessing _messageProcessing;
        private readonly ICallbackProcessing _callbackProcessing;
        
        public TelegramLogicHandlers(IUserRepository userRepository, IMessageProcessing messageProcessing, ICallbackProcessing callbackProcessing)
        {
            _messageProcessing = messageProcessing;
            _userRepository = userRepository;
            _callbackProcessing = callbackProcessing;
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => _messageProcessing.Processing(botClient, update.Message),
                UpdateType.EditedMessage => _messageProcessing.Processing(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => _callbackProcessing.Processing(botClient, update.CallbackQuery)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }
    }
}
