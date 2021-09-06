using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNoteBot
{
    public interface IUserRepository
    {
        User AddUser(long Id);
        User UpdateUser(long Id, UserState state);
        User GetUser(long Id);
    }
}
