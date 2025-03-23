using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class ProjectFile
{
    public int FileId { get; set; }

    public string? FileTitle { get; set; } = null!;

    public int? ItemId { get; set; }
    public virtual Item? Item { get; set; }

    public string FilePath { get; set; } = null!;

    public string? FileType { get; set; } = null!;

    public string? FileDescription { get; set; }

    public DateTime UploadedAt { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }

    public int? JournalNoteId { get; set; }

    public virtual JournalNote? JournalNote { get; set; }


}
