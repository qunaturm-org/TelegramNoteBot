using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramNoteBot.Handlers
{
    public class TelegramLogicHandlers
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageProcessing _messageProcessing;
        
        public TelegramLogicHandlers(IUserRepository userRepository, IMessageProcessing messageProcessing)
        {
            _messageProcessing = messageProcessing;
            _userRepository = userRepository;
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
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery)
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

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            User user = _userRepository.GetUser(message.From.Id);
            if (user == null)
            {
                _userRepository.AddUser(message.From.Id);
                user = _userRepository.GetUser(message.From.Id);
            }
            Task<Message> action;
            switch (user.State)
            {
                case UserState.Note:
                    {
                        action = _messageProcessing.AddNoteProcessing(botClient, message);
                        break;
                    }

                case UserState.InputnNoteIdToDelete:
                    {
                        action = _messageProcessing.DeleteNoteProcessing(botClient, message);
                        break;
                    }

                default:
                    {
                        action = (message.Text.Split(' ').First()) switch
                        {
                            "/inline" => _messageProcessing.SendInlineKeyboard(botClient, message),
                            _ => _messageProcessing.Usage(botClient, message)
                        };
                        break;
                    }
            }
            await action;
        }

        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            User user = _userRepository.GetUser(callbackQuery.Message.From.Id);
            var action = (callbackQuery.Data) switch
            {
                "functionsCallback" => TellMeAboutFunctional(),
                "createNotesCallback" => CreateNewNote(),
                "showNotesCallback" => _messageProcessing.GetAllNotes(botClient, callbackQuery.Message),
                "deteteNote" => _messageProcessing.DeleteNoteProcessing(botClient, callbackQuery.Message)
            };

            await action;

            async Task<Message> TellMeAboutFunctional()
            {
                return await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, "Создание заметок и пока что всё");
            }

            async Task CreateNewNote()
            {
                _userRepository.UpdateUser(callbackQuery.Message.Chat.Id, UserState.Note);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите свою заметку", replyMarkup: new ForceReplyMarkup { Selective = true });

            }
        }
    }
}
