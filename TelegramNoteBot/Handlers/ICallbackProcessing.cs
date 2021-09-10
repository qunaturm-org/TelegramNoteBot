using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramNoteBot.Handlers
{
    public interface ICallbackProcessing
    {
        Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message);
        Task<Message> GetAllNotes(ITelegramBotClient botClient, Message message);
        Task Processing(ITelegramBotClient botClient, CallbackQuery callbackQuery);
        Task<Message> TellMeAboutFunctional(ITelegramBotClient botClient, Message message);
        Task CreateNewNote(ITelegramBotClient botClient, Message message);
    }
}
