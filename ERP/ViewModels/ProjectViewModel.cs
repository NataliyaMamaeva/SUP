namespace ERP.ViewModels
{
    public class ProjectViewModel
    {
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public DateOnly Deadline { get; set; }
        public DateOnly? EventDate { get; set; }
        public int? ClientId { get; set; }
        public int? EmployeeId { get; set; }
        public decimal? EmployeePayment { get; set; }
        public DateOnly? PaymentDate {  get; set; } 
        public List<ItemViewModel>? Items { get; set; }
        public string? LayoutsRequired { get; set; }
        public bool? IsDocumentsComleted { get; set; }

        public List<ProjectFileUploadViewModel>? ProjectFiles { get; set; }
        public override string ToString()
        {
            return $"ProjectId: {ProjectId}, ProjectName: {ProjectName},IsDocumentsComleted: {IsDocumentsComleted}," +
                $"LayoutsRequired:{LayoutsRequired},  Deadline: {Deadline}, " +
                   $"ClientId: {ClientId}, EmployeeId: {EmployeeId}, EmployeePayment: {EmployeePayment}, " +
                   $"Items: [{string.Join(", ", Items ?? new List<ItemViewModel>())}], " +
                   $"ProjectFiles: [{string.Join(", ", ProjectFiles ?? new List<ProjectFileUploadViewModel>())}]";
        }
    }


    public class ProjectFileUploadViewModel
    {
        public string? ProjectName { get; set; }
        public IFormFile? FilePath { get; set; }
        // public string FileName { get; set; }
        //public string? FileType { get; set; }
        public override string ToString()
        {
            return $"ProjectName: {ProjectName}, FilePath: {(FilePath != null ? FilePath.FileName : "null")}";
        }
    }

    public class ItemViewModel
    {
        public int Index { get; set; } // Индекс элемента для связывания с файлом
        public string? ItemType { get; set; }

        public string? ItemName { get; set; }
        public int Amount { get; set; }
        public DateOnly? Deadline { get; set; }
        public decimal Price { get; set; }
        public string? Materials { get; set; }
        public string? Colors { get; set; }
        public string? ItemDescription { get; set; }
        public override string ToString()
        {
            return $"Index: {Index}, ItemType: {ItemType}, ItemName: {ItemName}, Amount: {Amount}, " +
                   $"Deadline: {Deadline?.ToString() ?? "null"}, Price: {Price}, Materials: {Materials}, " +
                   $"Colors: {Colors}, ItemDescription: {ItemDescription}";
        }
    }



    //public class ItemsFileUploadTest
    //{
    //    List<SketchUploadViewModel> Items { get; set; }
    //}

    public class ItemUploadViewModel
    {
        public int? ItemId { get; set; }
        public string? ProjectName { get; set; }    
        public IFormFile? Sketch { get; set; }
        public string? SketchPath { get; set; }
        public string? ItemType { get; set; }
        public string? ItemName { get; set; }  
        public int? Amount { get; set; }
        public decimal? Price { get; set; }
        public string? ItemDescription { get; set; }
        public string? Materials { get; set; }
        public string? Colors { get; set; }
        public DateOnly? Deadline { get; set; }
        public override string ToString()
        {
            return $"ItemId: {ItemId}, ItemType: {ItemType}, ItemName: {ItemName}, Amount: {Amount}, " +
                   $"Deadline: {Deadline?.ToString() ?? "null"}, Price: {Price}, Materials: {Materials}, " +
                   $"Colors: {Colors}, ItemDescription: {ItemDescription}, scetch fileName: " + Sketch?.FileName + "SketchPath: " + SketchPath;
        }
    }
}
