using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramNoteBot.Interfaces.Handlers;
using TelegramNoteBot.Interfaces.Repositories;
using TelegramNoteBot.Interfaces.RepositoriesS;
using TelegramNoteBot.Models;

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

            Models.User user = _userRepository.GetUser(message.From.Id);
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
                            "Обзор функций" => SendInlineKeyboard(botClient, message),
                            "/note" => SendNoteKeyboard(botClient, message),
                            "Заметки" => SendNoteKeyboard(botClient, message),
                            "/notification" => SendNotificationKeyboard(botClient, message),
                            "Уведомления" => SendNotificationKeyboard(botClient, message),
                            _ => Usage(botClient, message)
                        };
                        break;
                    }
            }
            await action;
        }
        public Task<Message> AddNoteProcessing(ITelegramBotClient botClient, Message message)
        {
            Note note = new Note
            {
                UserId = message.From.Id,
                NoteId = message.MessageId,
                Text = message.Text
            };
            _noteRepository.AddNewNote(note);
            _userRepository.UpdateUser(message.From.Id, UserState.Command);
            return botClient.SendTextMessageAsync(message.Chat.Id, "Заметка создана");
        }

        public async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var replyKeyboardMarkup = new ReplyKeyboardMarkup()
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Обзор функций"}, new KeyboardButton { Text = "Заметки"}, new KeyboardButton { Text = "Уведомления" } }
                },
                OneTimeKeyboard = false
            };


            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Выберите:",
                                                        replyMarkup: replyKeyboardMarkup);
        }
        public async Task<Message> SendNoteKeyboard(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Создать заметку", "createNotesCallback"),

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Мои заметки", "showNotesCallback"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Удалить заметку", "deteteNote")
                        }
                })
            {
                
            };

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Выберите:",
                                                        replyMarkup: inlineKeyboard);
        }

        public async Task<Message> SendNotificationKeyboard(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Создать напоминание", "createNotificationCallback")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Мои напоминания", "showNotificationCallback")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Удалить напоминание", "deleteNotificationCallback")
                        }
                });

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Выберите:",
                                                        replyMarkup: inlineKeyboard);
        }

        public async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/inline   - send inline keyboard\n" +
                                 "/note - send note keyboard\n" +
                                 "/notification - send notification keyboard";

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
