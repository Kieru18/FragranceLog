using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

public partial class Perfume
{
    [Key]
    public int PerfumeId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    public int? LaunchYear { get; set; }

    public int BrandId { get; set; }

    [StringLength(3)]
    public string CountryCode { get; set; } = null!;

    [ForeignKey("BrandId")]
    [InverseProperty("Perfumes")]
    public virtual Brand Brand { get; set; } = null!;

    [ForeignKey("CountryCode")]
    [InverseProperty("Perfumes")]
    public virtual Country CountryCodeNavigation { get; set; } = null!;

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeDaytimeVote> PerfumeDaytimeVotes { get; set; } = new List<PerfumeDaytimeVote>();

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeGenderVote> PerfumeGenderVotes { get; set; } = new List<PerfumeGenderVote>();

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeLongevityVote> PerfumeLongevityVotes { get; set; } = new List<PerfumeLongevityVote>();

    [InverseProperty("Perfume")]
    public virtual PerfumePhoto? PerfumePhoto { get; set; }

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeSeasonVote> PerfumeSeasonVotes { get; set; } = new List<PerfumeSeasonVote>();

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeSillageVote> PerfumeSillageVotes { get; set; } = new List<PerfumeSillageVote>();

    [InverseProperty("Perfume")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("PerfumeId")]
    [InverseProperty("Perfumes")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    [ForeignKey("PerfumeId")]
    [InverseProperty("Perfumes")]
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
