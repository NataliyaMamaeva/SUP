using ERP.Models;

namespace ERP.ViewModels
{
    public class JournalTopicViewModel
    {
        public class JournalTopic
        {
            public int JournalTopicId { get; set; }
            public string JournalTopicName { get; set; }

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
        }
    }
}
