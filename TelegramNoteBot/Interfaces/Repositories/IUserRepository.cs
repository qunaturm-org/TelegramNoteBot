using TelegramNoteBot.Models;

namespace TelegramNoteBot.Interfaces.RepositoriesS
{
    public interface IUserRepository
    {
        User AddUser(long Id);
        User UpdateUser(long Id, UserState state);
        User GetUser(long Id);
    }
}
