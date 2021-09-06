﻿namespace TelegramNoteBot
{
    public interface INoteRepository
    {
        void AddNewNote(Note note);
        List<Note> GetAllNotes(long userId);
    }
}