using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramNoteBot.Handlers
{
    public class CallbackProcessing : ICallbackProcessing
    {
        private readonly IUserRepository _userRepository;
        private readonly INoteRepository _noteRepository;
        public CallbackProcessing(IUserRepository userRepository, INoteRepository noteRepository)
        {
            _userRepository = userRepository;
            _noteRepository = noteRepository;
        }
        public async Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message)
        {
            _userRepository.UpdateUser(message.Chat.Id, UserState.DeleteNote);
            return await botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер заметки", replyMarkup: new ForceReplyMarkup { Selective = true });
        }

        public async Task<Message> GetAllNotes(ITelegramBotClient botClient, Message message)
        {
            var notes = _noteRepository.GetAllNotes(message.Chat.Id);
            if (!notes.Any())
            {
                return await botClient.SendTextMessageAsync(message.Chat.Id, "У вас ещё нет сохранённых заметок");
            }
            int counter = 1;
            var formatedNotes = notes.Select(note =>
                $"Заметка {counter++}\n{note.Text}\n");
            return await botClient.SendTextMessageAsync(message.Chat.Id, string.Join("----------\n", formatedNotes));
        }

        public async Task<Message> TellMeAboutFunctional(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Создание заметок и пока что всё");

        }

        public async Task CreateNewNote(ITelegramBotClient botClient, Message message)
        {
            _userRepository.UpdateUser(message.Chat.Id, UserState.AddNote);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите свою заметку", replyMarkup: new ForceReplyMarkup { Selective = true });

        }
        public async Task Processing(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            User user = _userRepository.GetUser(callbackQuery.Message.From.Id);
            var action = (callbackQuery.Data) switch
            {
                "functionsCallback" => TellMeAboutFunctional(botClient, callbackQuery.Message),
                "createNotesCallback" => CreateNewNote(botClient, callbackQuery.Message),
                "showNotesCallback" => GetAllNotes(botClient, callbackQuery.Message),
                "deteteNote" => DeleteNoteProcessing(botClient, callbackQuery.Message)
            };

            await action;
        }
    }
}
