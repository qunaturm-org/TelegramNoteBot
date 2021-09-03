﻿using System;
using System.Threading;
using Telegram.Bot;
using MongoDB.Driver;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Examples.Echo;

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
            NoteRepository noteRepository = new NoteRepository(notesCollection);

            var handlers = new Handlers(noteRepository);
            client.StartReceiving(new DefaultUpdateHandler(handlers.HandleUpdateAsync, handlers.HandleErrorAsync),
                   cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}
