using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "UserId")]
public partial class PerfumeSeasonVote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int UserId { get; set; }

    public int SeasonId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VoteDate { get; set; }

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeSeasonVotes")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("SeasonId")]
    [InverseProperty("PerfumeSeasonVotes")]
    public virtual Season Season { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeSeasonVotes")]
    public virtual User User { get; set; } = null!;
}
