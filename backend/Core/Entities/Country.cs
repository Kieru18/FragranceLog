using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Countries_Name", IsUnique = true)]
public partial class Country
{
    [Key]
    [StringLength(3)]
    public string Code { get; set; } = null!;

    [StringLength(52)]
    public string Name { get; set; } = null!;

    [InverseProperty("CountryCodeNavigation")]
    public virtual ICollection<Perfume> Perfumes { get; set; } = new List<Perfume>();
}
