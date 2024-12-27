using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.FileIO;

namespace ERP.ViewModels
{
    public class AddPhotosViewModel
    {     
      public int projectId {  get; set; }
      public string fileType { get; set; }
      public int? itemPointer {  get; set; }
      public  IFormFile photo {  get; set; }

        public override string ToString()
        {
            return $"ProjectId: {projectId}, FileType: {fileType}, ItemPointer: {itemPointer}, Photo: {photo.FileName}";
        }
    }
}
