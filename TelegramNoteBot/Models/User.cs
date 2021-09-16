using MongoDB.Bson.Serialization.Attributes;

namespace TelegramNoteBot.Models
{
    public class User
    {
        [BsonId]
        public long Id { get; set; }
        public UserState State { get; set; }
    }
}