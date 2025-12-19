using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Groups_Name", IsUnique = true)]
public partial class Group
{
    [Key]
    public int GroupId { get; set; }

    [StringLength(30)]
    public string Name { get; set; } = null!;

    [ForeignKey("GroupId")]
    [InverseProperty("Groups")]
    public virtual ICollection<Perfume> Perfumes { get; set; } = new List<Perfume>();
}
