using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Table("Longevity")]
[Index("Name", Name = "UQ_Longevity_Name", IsUnique = true)]
public partial class Longevity
{
    [Key]
    public int LongevityId { get; set; }

    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Longevity")]
    public virtual ICollection<PerfumeLongevityVote> PerfumeLongevityVotes { get; set; } = new List<PerfumeLongevityVote>();
}
