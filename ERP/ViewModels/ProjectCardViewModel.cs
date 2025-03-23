using ERP.Models;

namespace ERP.ViewModels
{
    public class ProjectCardViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string?  ProjectColor { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public DateOnly? EventDate { get; set; }
        public DateOnly? Deadline { get; set; }
        public string? ClientName { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? PaymentTotal { get; set; }
        public decimal? AdvanceRate { get; set; }
        public decimal? EmployeePayment { get; set; }
        public string? Description { get; set; }

        public string? LayoutsRequired { get; set; }
        public bool IsDocumentsComleted { get; set; }

        public List<ItemCardViewModel> Items { get; set; } = new();
        public List<JournalNote> JournalNotes { get; set; } = new();
        public List<FileCardViewModel> Layouts { get; set; } = new();
        public List<FileCardViewModel> Documents { get; set; } = new();
        public List<FileCardViewModel> Gallery { get; set; } = new();
        public List<FileCardViewModel> JournalPhotos { get; set; } = new();
        public int? SelectedClientId { get; set; }
        public int? SelectedEmployeeId { get; set; }
     

        public override string ToString()
        {
            return $"Project ID: {ProjectId}\n" +
                   $"Project Name: {ProjectName}\n" +
                   $"Payment Date: {PaymentDate?.ToString() ?? "N/A"}\n" +
                   $"Event Date: {EventDate?.ToString() ?? "N/A"}\n" +
                   $"Deadline: {Deadline?.ToString() ?? "N/A"}\n" +
                   $"Client Name: {ClientName ?? "N/A"}\n" +
                   $"Employee Name: {EmployeeName ?? "N/A"}\n" +
                   $"Payment Total: {PaymentTotal?.ToString("C") ?? "N/A"}\n" +
                   $"Advance Rate: {AdvanceRate?.ToString("P") ?? "N/A"}\n" +
                    $"Employee Payment: {EmployeePayment?.ToString("P") ?? "N/A"}\n" +
                   $"Description: {Description ?? "N/A"}\n" +
                   $"Items:\n{string.Join("\n", Items)}\n" +
                   $"Layouts:\n{string.Join("\n", Layouts)}\n" +
                   $"Documents:\n{string.Join("\n", Documents)}\n" +
                   $"Gallery:\n{string.Join("\n", Gallery)}\n" +
                   $"Journal Photos:\n{string.Join("\n", JournalPhotos)}";
        }
    }

    public class ItemCardViewModel
    {
        public int? ItemId { get; set; }
        public string ItemType { get; set; } = null!;
        public string? ItemName { get; set; }
        public DateOnly? Deadline { get; set; }
        public decimal? Price { get; set; }
        public string? SketchPath { get; set; }
        public int? Amount { get; set; }
        public string? ItemDescription { get; set; }
        public string? SelectedMaterials { get; set; }
        public string? SelectedColors { get; set; }
        public override string ToString()
        {
            return $"ItemId: {ItemId}, ItemType: {ItemType}, ItemName: {ItemName}, Deadline: {Deadline}, " +
                   $"Price: {Price}, SketchPath: {SketchPath}, Amount: {Amount}, " +
                   $"ItemDescription: {ItemDescription}, SelectedMaterials: {SelectedMaterials}, SelectedColors: {SelectedColors}";
        }
    }

    public class FileCardViewModel
    {
        public int? FileId { get; set; }
        public string FileTitle { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public override string ToString()
        {
            return $"FileId: {FileId}, FileTitle: {FileTitle}, FilePath: {FilePath}, UploadedAt: {UploadedAt:dd.MM.yyyy HH:mm}";
        }
    }

}
