using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TelegramNoteBot.Models
{
    public class Notification
    {
        [BsonId]
        public string Id { get { return $"{UserId}:{NotificationId}"; } }
        public long UserId { get; set; }
        public long NotificationId { get; set; }
        public string Text { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}
