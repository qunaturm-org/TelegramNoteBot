using System.Collections.Generic;
using TelegramNoteBot.Models;

namespace TelegramNoteBot.Interfaces.Repositories
{
    public interface INoteRepository
    {
        void AddNewNote(Note note);
        List<Note> GetAllNotes(long userId);
        void DeleteNote(long userId, int noteNumber);
    }
}