using MongoDB.Driver;
using TelegramNoteBot.Handlers;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

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
                            .AddSingleton<IConfiguration>(configuration)
                            .AddSingleton<TelegramLogicHandlers>()
                            .AddSingleton<ICallbackProcessing, CallbackProcessing>()
                            .AddSingleton<IMessageProcessing, MessageProcessing>()
                            .AddSingleton<IUserRepository, UserRepository>()
                            .AddSingleton<INoteRepository, NoteRepository>()
                            .AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(x.GetService<IConfiguration>().GetValue<string>("BotToken")))
                            .AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(x.GetService<IConfiguration>().GetValue<string>("MongoDbConnectionString")))
                            .AddSingleton<IMongoDatabase>(x => x.GetService<IMongoClient>().GetDatabase("TGBotDB"))
                            .AddSingleton<IMongoCollection<User>>(x => x.GetService<IMongoDatabase>().GetCollection<User>("Users"))
                            .AddSingleton<IMongoCollection<Note>>(x => x.GetService<IMongoDatabase>().GetCollection<Note>("Notes")));
    }
}
