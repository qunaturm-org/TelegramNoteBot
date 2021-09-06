using System;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramNoteBot;

namespace Telegram.Bot.Examples.Echo
{
    public class HandlersToDelete
    {
        private NoteRepository _noteRepository;
        private ConcurrentDictionary<long, UserState> _userInfo;
        
        public HandlersToDelete(NoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
            _userInfo = new ConcurrentDictionary<long, UserState>();
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
#pragma warning disable IDE0072 // Add missing cases
            var handler = update.Type switch
#pragma warning restore IDE0072 // Add missing cases
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
                _ => SomeFuckingErrorWinhSwitch(botClient, update.Message)
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

        private async Task SomeFuckingErrorWinhSwitch(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine("произошёл казус");
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            User user = _userRepository; 
            if (_userInfo.GetOrAdd(message.From.Id, UserState.Command).Equals(UserState.Command))
            {
                _userInfo.AddOrUpdate(message.From.Id, UserState.Command, (x, y) => UserState.Command);
            }

            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            Task<Message> action;
            
            if (_userInfo.GetOrAdd(message.From.Id, UserState.Command).Equals(UserState.Note))
            {
                action = AddNoteToDB(botClient, message);
            }
            else if (_userInfo.GetOrAdd(message.From.Id, UserState.Command).Equals(UserState.InputnNoteIdToDelete))
            {
                action = AddNoteToDB(botClient, message);
                _userInfo.AddOrUpdate(message.Chat.Id, UserState.Note, (x, y) => UserState.Note); //?
            }
            else
            {
                action = (message.Text.Split(' ').First()) switch
                {
                    "/inline" => SendInlineKeyboard(botClient, message),
                    "/remove" => RemoveKeyboard(botClient, message),
                    _ => Usage(botClient, message)
                };
            }

            var sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

            async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Обзор функций", "functionsCallback"),
                            InlineKeyboardButton.WithCallbackData("Создать заметку", "createNotesCallback"),

                    },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Мои заметки", "showNotesCallback"),
                            InlineKeyboardButton.WithCallbackData("Удалить заметку", "deteteNote")
                        },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Выберите:",
                                                            replyMarkup: inlineKeyboard);
            }

            async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Removing keyboard",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }

            async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/inline   - send inline keyboard\n";// +
                                     //"/remove   - remove custom keyboard\n";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private async Task<Message> AddNoteToDB(ITelegramBotClient botClient, Message message)
        {
            Note note = new Note(message.From.Id, message.MessageId, message.Text, false);
            _noteRepository.AddNewNote(note);
            _userInfo.AddOrUpdate(message.Chat.Id, UserState.Command, (x, y) => UserState.Command);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Заметка создана");
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