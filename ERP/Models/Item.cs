using System;
using System.Collections.Generic;

namespace ERP.Models;


public partial class Item
{
    public int ItemId { get; set; }

    public int ProjectId { get; set; }

    public string ItemType { get; set; } = null!;
    public string? ItemName { get; set; } 

    public string? SketchPath { get; set; }

    public int? Amount { get; set; }

    public decimal? Price { get; set; }

    public string? ItemDescription { get; set; }

    public string? Materials { get; set; }

    public string? Colors { get; set; }

    public DateOnly? Deadline { get; set; }

    public virtual Project Project { get; set; } = null!;

    //public virtual ProjectFile? SketchNavigation { get; set; }

    public override string ToString()
    {
        return $"Item ID: {ItemId}\n" +
               $"Project ID: {ProjectId}\n" +
               $"Item Type: {ItemType}\n" +
               $"Sketch ID: {SketchPath ?? "N/A"}\n" +
               $"Amount: {Amount?.ToString() ?? "N/A"}\n" +
               $"Price: {Price}\n" +
               $"Description: {ItemDescription ?? "N/A"}\n" +
               $"Materials: {Materials ?? "N/A"}\n" +
               $"Colors: {Colors ?? "N/A"}\n" +
               $"Deadline: {Deadline?.ToString("yyyy-MM-dd") ?? "N/A"}";
    }
}
