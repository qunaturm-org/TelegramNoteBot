using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramNoteBot.Handlers
{
    public class MessageProcessing : IMessageProcessing
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        public MessageProcessing(INoteRepository noteRepository, IUserRepository userRepository)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
        }

        public async Task Processing(ITelegramBotClient botClient, Message message)
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
                case UserState.AddNote:
                    {
                        action = AddNoteProcessing(botClient, message);
                        break;
                    }

                case UserState.DeleteNote:
                    {
                        action = DeleteNoteProcessing(botClient, message);
                        break;
                    }

                default:
                    {
                        action = (message.Text.Split(' ').First()) switch
                        {
                            "/inline" => SendInlineKeyboard(botClient, message),
                            _ => Usage(botClient, message)
                        };
                        break;
                    }
            }
            await action;
        }
        public Task<Message> AddNoteProcessing(ITelegramBotClient botClient, Message message)
        {
            Note note = new Note(message.From.Id, message.MessageId, message.Text, false);
            _noteRepository.AddNewNote(note);
            _userRepository.UpdateUser(message.From.Id, UserState.Command);
            return botClient.SendTextMessageAsync(message.Chat.Id, "Заметка создана");
        }

        public async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
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

        public async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/inline   - send inline keyboard\n";

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: usage,
                                                        replyMarkup: new ReplyKeyboardRemove());
        }

        public Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message)
        {
            _noteRepository.DeleteNote(message.From.Id, int.Parse(message.Text));
            _userRepository.UpdateUser(message.From.Id, UserState.Command);
            return botClient.SendTextMessageAsync(message.Chat.Id, "Заметка удалена");
        }
    }
}
