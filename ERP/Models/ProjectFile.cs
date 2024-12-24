using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class ProjectFile
{
    public int FileId { get; set; }

    public string? FileTitle { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? FileType { get; set; } = null!;

    public string? FileDescription { get; set; }

    public DateTime UploadedAt { get; set; }

    public int? ProjectId { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual Project? Project { get; set; }
}
