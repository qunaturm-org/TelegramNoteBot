using Telegram.Bot;
using MongoDB.Driver;
using Telegram.Bot.Extensions.Polling;
using System.Threading;
using System;
using TelegramNoteBot.Handlers;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace TelegramNoteBot
{
    class Program
    {
        public static Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            return CreateHostBuilder(args, configuration).Build().RunAsync();
        }
        static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services
                            .AddHostedService<TelegramBotWorker>()
                            .AddSingleton<TelegramBotClient>()
                            .AddSingleton<MongoClient>()
                            .AddSingleton<IMongoDatabase>()
                            .AddSingleton<IMongoCollection<Note>>()
                            .AddSingleton<IMongoCollection<User>>()
                            .AddSingleton<INoteRepository, NoteRepository>()
                            .AddSingleton<IUserRepository, UserRepository>()
                            .AddSingleton<ICallbackProcessing>()
                            .AddSingleton<IMessageProcessing>());
    }
}
