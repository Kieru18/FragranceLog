using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("Username", Name = "UQ_Users_Username", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Password { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<PerfumeDaytimeVote> PerfumeDaytimeVotes { get; set; } = new List<PerfumeDaytimeVote>();

    [InverseProperty("User")]
    public virtual ICollection<PerfumeGenderVote> PerfumeGenderVotes { get; set; } = new List<PerfumeGenderVote>();

    [InverseProperty("User")]
    public virtual ICollection<PerfumeLongevityVote> PerfumeLongevityVotes { get; set; } = new List<PerfumeLongevityVote>();

    [InverseProperty("User")]
    public virtual ICollection<PerfumeSeasonVote> PerfumeSeasonVotes { get; set; } = new List<PerfumeSeasonVote>();

    [InverseProperty("User")]
    public virtual ICollection<PerfumeSillageVote> PerfumeSillageVotes { get; set; } = new List<PerfumeSillageVote>();

    [InverseProperty("User")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
