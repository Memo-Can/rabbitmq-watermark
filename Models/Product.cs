using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watermark.Models;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; }
    
    [Column(TypeName="decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(1,100)]
    public int Stock {get; set; }

    [StringLength(150)]
    public string ImageName { get; set; }
}
