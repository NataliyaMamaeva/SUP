namespace ERP.Models
{
    public class Material
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
    }

    public class Color
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; }
    }

    public class ItemsType
    {
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
    }
}
