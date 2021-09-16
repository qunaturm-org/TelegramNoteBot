using MongoDB.Bson.Serialization.Attributes;

namespace TelegramNoteBot.Models
{
    public class Note
    {
        [BsonId]
        public string Id { get { return $"{UserId}:{NoteId}"; } }
        public long UserId { get; set; }
        public long NoteId { get; set; }

        public string Text { get; set; }
    }
}