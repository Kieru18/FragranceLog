using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Seasons_Name", IsUnique = true)]
public partial class Season
{
    [Key]
    public int SeasonId { get; set; }

    [StringLength(6)]
    public string Name { get; set; } = null!;

    [InverseProperty("Season")]
    public virtual ICollection<PerfumeSeasonVote> PerfumeSeasonVotes { get; set; } = new List<PerfumeSeasonVote>();
}
