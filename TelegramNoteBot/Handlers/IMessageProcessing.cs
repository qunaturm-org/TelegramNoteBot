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
using Telegram.Bot;

namespace TelegramNoteBot.Handlers
{
    public interface IMessageProcessing
    {
        Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message);
        Task<Message> Usage(ITelegramBotClient botClient, Message message);
        Task<Message> AddNoteProcessing(ITelegramBotClient botClient, Message message);
        Task<Message> DeleteNoteProcessing(ITelegramBotClient botClient, Message message);

    }
}
