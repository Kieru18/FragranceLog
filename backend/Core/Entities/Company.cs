using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Companies_Name", IsUnique = true)]
public partial class Company
{
    [Key]
    public int CompanyId { get; set; }

    [StringLength(60)]
    public string Name { get; set; } = null!;

    [InverseProperty("Company")]
    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
}
