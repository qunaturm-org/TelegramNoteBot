using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using TelegramNoteBot.Interfaces.Repositories;
using TelegramNoteBot.Models;

namespace TelegramNoteBot.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _notifications;
    public NotificationRepository(IMongoCollection<Notification> notifications)
    {
        _notifications = notifications;
    }
    public void AddNewNotification(Notification notification)
    {
        _notifications.InsertOne(notification);
    }

    public void DeleteNotification(long userId, int notificationNumber)
    {
        var notificationToDelete = _notifications.AsQueryable<Notification>().Where(x => x.UserId == userId).Skip(notificationNumber - 1).FirstOrDefault();
        _notifications.DeleteOne(x => x.Id == notificationToDelete.Id);
    }

    public List<Notification> GetAllNotifications(long userId)
    {
        return _notifications.AsQueryable().Where(x => x.UserId == userId).ToList();
    }
}
