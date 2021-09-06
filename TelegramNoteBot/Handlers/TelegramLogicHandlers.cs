using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var action = (callbackQuery.Data) switch
            {
                "functionsCallback" => TellMeAboutFunctional(),
                "createNotesCallback" => CreateNewNote(),
                "showNotesCallback" => GetNotes(),
                "deteteNote" => DeleteNote()
            };

            await action;

            async Task<Message> TellMeAboutFunctional()
            {
                return await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, "Создание заметок и пока что всё");
            }

            async Task CreateNewNote()
            {
                _userInfo.AddOrUpdate(callbackQuery.Message.Chat.Id, UserState.Note, (x, y) => UserState.Note);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите свою заметку", replyMarkup: new ForceReplyMarkup { Selective = true });

            }

            async Task<Message> GetNotes()
            {
                UserState value = UserState.Command;
                var notes = _noteRepository.GetAllNotes(callbackQuery.Message.Chat.Id);
                if (!notes.Any())
                {
                    return await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "У вас ещё нет сохранённых заметок");
                }
                var formatedNotes = notes.Select(note =>
                    $"Text: {note.Text}\n");
                var response = string.Join("----------\n", formatedNotes);
                return await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, response);
            }

            async Task<Message> DeleteNote()
            {
                UserState value = UserState.Command;
                int counter = 1;
                if (_userInfo.TryGetValue(callbackQuery.Message.Chat.Id, out value))
                {
                    var notes = _noteRepository.GetAllNotes(callbackQuery.Message.Chat.Id);
                    botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите номер заметки, которую хотите удалить");
                    var formatedNotes = notes.Select(note =>
                        $"Номер заметки: {counter++}\n Text: {note.Text}\n");
                    var response = string.Join("----------\n", formatedNotes);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, response, replyMarkup: new ForceReplyMarkup { Selective = true });
                    _userInfo.AddOrUpdate(callbackQuery.Message.Chat.Id, UserState.Note, (x, y) => UserState.InputnNoteIdToDelete);
                }
                return await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Чтобы что-то удалить надо сначала это что-то сохранить");
            }

        }
    }
}
