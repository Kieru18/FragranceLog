using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "UserId")]
public partial class PerfumeLongevityVote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int UserId { get; set; }

    public int LongevityId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VoteDate { get; set; }

    [ForeignKey("LongevityId")]
    [InverseProperty("PerfumeLongevityVotes")]
    public virtual Longevity Longevity { get; set; } = null!;

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeLongevityVotes")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeLongevityVotes")]
    public virtual User User { get; set; } = null!;
}
