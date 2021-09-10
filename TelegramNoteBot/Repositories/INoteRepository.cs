using System.Collections.Generic;

namespace TelegramNoteBot
{
    public interface INoteRepository
    {
        void AddNewNote(Note note);
        List<Note> GetAllNotes(long userId);
        void DeleteNote(long userId, int noteNumber);
    }
}