using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace TelegramNoteBot
{
    public class NoteRepository : INoteRepository
    {
        private IMongoCollection<Note> _notesCollection;
        public NoteRepository(IMongoCollection<Note> notesCollection)
        {
            _notesCollection = notesCollection;
        }

        public void AddNewNote(Note note)
        {
            _notesCollection.InsertOne(note);
        }

        public void DeleteNote(long userId, int noteNumber)
        {
            var noteToDelete = _notesCollection.AsQueryable().Where(u => u.UserId == userId).Skip(noteNumber - 1).FirstOrDefault();
            _notesCollection.DeleteOne(u => u.Id == noteToDelete.Id);
        }

        public List<Note> GetAllNotes(long userId)
        {
            var docs = _notesCollection.AsQueryable().Where(u => u.UserId == userId).ToList();
            return docs;
        }
    }
}
