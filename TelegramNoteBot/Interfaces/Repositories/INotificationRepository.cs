using System.Collections.Generic;
using TelegramNoteBot.Models;

namespace TelegramNoteBot.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        void AddNewNotification(Notification notification);
        List<Notification> GetAllNotifications(long userId);
        void DeleteNotification(long userId, int notificationNumber);
    }
}
