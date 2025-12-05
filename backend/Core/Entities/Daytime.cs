using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Daytimes_Name", IsUnique = true)]
public partial class Daytime
{
    [Key]
    public int DaytimeId { get; set; }

    [StringLength(5)]
    public string Name { get; set; } = null!;

    [InverseProperty("Daytime")]
    public virtual ICollection<PerfumeDaytimeVote> PerfumeDaytimeVotes { get; set; } = new List<PerfumeDaytimeVote>();
}
