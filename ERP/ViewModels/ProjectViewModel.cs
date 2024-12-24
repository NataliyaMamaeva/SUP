namespace ERP.ViewModels
{
    public class ProjectViewModel
    {
        public string? ProjectName { get; set; }
        public DateOnly? Deadline { get; set; }
        public int? ClientId { get; set; }
        public int? EmployeeId { get; set; }
        public List<ItemViewModel>? Items { get; set; }

        public List<ProjectFileUploadViewModel>? ProjectFiles { get; set; }
    }


    public class ProjectFileUploadViewModel
    {
        public string? ProjectName { get; set; }
        public IFormFile? FilePath { get; set; }
        // public string FileName { get; set; }
        //public string? FileType { get; set; }
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
    }



    //public class ItemsFileUploadTest
    //{
    //    List<SketchUploadViewModel> Items { get; set; }
    //}

    public class ItemUploadViewModel
    {
        public string? ProjectName { get; set; }    
        public IFormFile? Sketch { get; set; }
        public string? ItemType { get; set; }
        public string? ItemName { get; set; }  
        public int? Amount { get; set; }
        public decimal? Price { get; set; }
        public string? ItemDescription { get; set; }
        public string? Materials { get; set; }
        public string? Colors { get; set; }
        public DateOnly? Deadline { get; set; }

    }

}
