using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "UserId")]
public partial class PerfumeSillageVote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int UserId { get; set; }

    public int SillageId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VoteDate { get; set; }

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeSillageVotes")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("SillageId")]
    [InverseProperty("PerfumeSillageVotes")]
    public virtual Sillage Sillage { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeSillageVotes")]
    public virtual User User { get; set; } = null!;
}
