using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models;

public partial class Requisite
{
    public int RequisiteId { get; set; }

    public string FileTitle { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? Comment { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? UploadedAt { get; set; }

    public int? ClientId { get; set; }

    public virtual Client? Client { get; set; }
}
