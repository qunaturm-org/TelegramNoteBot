using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramNoteBot.Handlers
{
    public interface IMessageProcessing
    {
        Task Processing(ITelegramBotClient botClient, Message message);
        Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message);
        Task<Message> Usage(ITelegramBotClient botClient, Message message);
        Task<Message> AddNoteProcessing(ITelegramBotClient botClient, Message message);
        Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message);
    }
}
