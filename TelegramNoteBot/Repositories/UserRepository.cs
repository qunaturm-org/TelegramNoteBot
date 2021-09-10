using MongoDB.Driver;
using System.Linq;

namespace TelegramNoteBot
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        public UserRepository(IMongoCollection<User> userCollection)
        {
            _usersCollection = userCollection;
        }

        public User AddUser(long Id)
        {
            var user = new User
            {
                Id = Id,
                State = UserState.Command
            };
            _usersCollection.InsertOne(user);
            return user;
        }

        public User UpdateUser(long Id, UserState state)
        {
            var update = Builders<User>.Update.Set(field => field.State, state);
            var updateResult = _usersCollection.UpdateOne(user => user.Id == Id, update);
            return GetUser(Id);
        }

        public User GetUser(long Id)
        {
            return _usersCollection.AsQueryable().Where(user => user.Id == Id).SingleOrDefault();
        }
    }
}
