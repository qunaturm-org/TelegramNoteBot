using Telegram.Bot;
using MongoDB.Driver;
using Telegram.Bot.Extensions.Polling;
using System.Threading;
using System;
using TelegramNoteBot.Handlers;

namespace TelegramNoteBot
{
    class Program
    {
        private static string token { get; set; } = "1909541944:AAEVkpxs19CMI3WCy5WT8ruYX6goLeETfNI";
        private static TelegramBotClient client;

        public static void Main(string[] args)
        {
            client = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();

            MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
            IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
            IMongoCollection<Note> notesCollection = database.GetCollection<Note>("Notes");
            IMongoCollection<User> usersCollection = database.GetCollection<User>("Users");
            INoteRepository noteRepository = new NoteRepository(notesCollection);
            IUserRepository userRepository = new UserRepository(usersCollection);
            ICallbackProcessing callbackProcessing = new CallbackProcessing(userRepository, noteRepository);
            IMessageProcessing messageProcessing = new MessageProcessing(noteRepository, userRepository);

            var handlers = new TelegramLogicHandlers(userRepository, messageProcessing, callbackProcessing);
            client.StartReceiving(new DefaultUpdateHandler(handlers.HandleUpdateAsync, handlers.HandleErrorAsync),
                   cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}
