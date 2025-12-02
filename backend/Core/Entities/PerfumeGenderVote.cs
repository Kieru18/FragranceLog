using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "UserId")]
public partial class PerfumeGenderVote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int UserId { get; set; }

    public int GenderId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VoteDate { get; set; }

    [ForeignKey("GenderId")]
    [InverseProperty("PerfumeGenderVotes")]
    public virtual Gender Gender { get; set; } = null!;

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeGenderVotes")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeGenderVotes")]
    public virtual User User { get; set; } = null!;
}
