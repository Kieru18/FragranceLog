using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Brands_Name", IsUnique = true)]
public partial class Brand
{
    [Key]
    public int BrandId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public int CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Brands")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Brand")]
    public virtual ICollection<Perfume> Perfumes { get; set; } = new List<Perfume>();
}
