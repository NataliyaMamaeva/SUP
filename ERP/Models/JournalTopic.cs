namespace ERP.Models
{
    public class JournalTopic
    {
        public int JournalTopicId { get; set; }
        public string JournalTopicName { get; set; }
        public virtual ICollection<JournalNote>? JournalNotes { get; set; } = new List<JournalNote>();
        public override string ToString()
        {
            return $"JournalTopicId: {JournalTopicId}, JournalTopicName: {JournalTopicName}, NotesCount: {JournalNotes?.Count ?? 0}";
        }

    }

    public class JournalNote
    {
        public int JournalNoteId { get; set; }
        public string JournalNoteDescription { get; set; }
        public string? JournalTopicName { get; set; }
        public int? JournalTopicId { get; set; }
        public virtual JournalTopic? JournalTopic { get; set; }
        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        public virtual ICollection<ProjectFile>? Photos { get; set; } = new List<ProjectFile>();
        public override string ToString()
        {
            return $"JournalNoteId: {JournalNoteId}, Description: {JournalNoteDescription}, JournalTopicName: {JournalTopicName}, ProjectId: {ProjectId}, PhotosCount: {Photos?.Count ?? 0}";
        }
    }



}
