using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "UserId")]
public partial class PerfumeDaytimeVote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int UserId { get; set; }

    public int DaytimeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VoteDate { get; set; }

    [ForeignKey("DaytimeId")]
    [InverseProperty("PerfumeDaytimeVotes")]
    public virtual Daytime Daytime { get; set; } = null!;

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeDaytimeVotes")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeDaytimeVotes")]
    public virtual User User { get; set; } = null!;
}
