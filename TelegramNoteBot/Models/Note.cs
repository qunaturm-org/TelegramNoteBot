using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TelegramNoteBot
{
    public class Note
    {
        [BsonId]
        public string Id { get { return $"{UserId}:{NoteId}"; } }
        public long UserId { get; set; }
        public long NoteId { get; set; }

        public string Text { get; set; }
        public bool IsRemind { get; set; } = false;

        public DateTime ScheduledTime { get; set; }
        public Note(long UserId, long NoteId, string Text, bool IsRemind)
        {
            this.UserId = UserId;
            this.NoteId = NoteId;
            this.Text = Text;
            this.IsRemind = IsRemind;
        }

    }
}
