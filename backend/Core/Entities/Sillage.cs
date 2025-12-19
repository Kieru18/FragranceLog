using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Table("Sillage")]
[Index("Name", Name = "UQ_Sillage_Name", IsUnique = true)]
public partial class Sillage
{
    [Key]
    public int SillageId { get; set; }

    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Sillage")]
    public virtual ICollection<PerfumeSillageVote> PerfumeSillageVotes { get; set; } = new List<PerfumeSillageVote>();
}
