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

        public async Task<Message> GetAllNotes(ITelegramBotClient botClient, Message message)
        {
            var notes = _noteRepository.GetAllNotes(message.From.Id);
            if (!notes.Any())
            {
                return await botClient.SendTextMessageAsync(message.Chat.Id, "У вас ещё нет сохранённых заметок");
            }
            var formatedNotes = notes.Select(note =>
                $"Text: {note.Text}\n");
            return await botClient.SendTextMessageAsync(message.Chat.Id, string.Join("----------\n", formatedNotes));
        }

        public Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message)
        {
            UserState value = UserState.Command;
            int counter = 1;
            if (_userRepository.GetUser(message.Chat.Id).State == UserState.InputnNoteIdToDelete)
            {
                var notes = _noteRepository.GetAllNotes(message.Chat.Id);
                botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер заметки, которую хотите удалить");
                var formatedNotes = notes.Select(note =>
                    $"Номер заметки: {counter++}\n Text: {note.Text}\n");
                var response = string.Join("----------\n", formatedNotes);
                botClient.SendTextMessageAsync(message.Chat.Id, response, replyMarkup: new ForceReplyMarkup { Selective = true });
                _userRepository.UpdateUser(message.From.Id, UserState.InputnNoteIdToDelete);
            }
            return botClient.SendTextMessageAsync(message.Chat.Id, "Чтобы что-то удалить надо сначала это что-то сохранить");
        }
    }
}
