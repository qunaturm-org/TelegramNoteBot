using System;
using System.Collections.Generic;
using TelegramNoteBot.Interfaces.Handlers;

namespace TelegramNoteBot.Handlers
{
    internal class NotificationProcessing : INotificationProcessing
    {
        private readonly Queue<Models.Notification> _queueNotifications;
        public NotificationProcessing(Queue<Models.Notification> queueNotifications)
        {
            _queueNotifications = queueNotifications;
        }

        public void AddNotificicationToQueue(long userId, Models.Notification note)
        {
            _queueNotifications.Enqueue(note);
            throw new NotImplementedException();

        }

    }
}
