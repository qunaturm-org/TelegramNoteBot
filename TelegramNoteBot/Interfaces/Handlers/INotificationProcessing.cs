namespace TelegramNoteBot.Interfaces.Handlers
{
    interface INotificationProcessing
    {
        void AddNotificicationToQueue(long userId, Models.Notification notification);
    }
}
