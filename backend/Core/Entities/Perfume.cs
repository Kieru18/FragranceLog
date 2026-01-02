using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("BrandId", Name = "IX_Perfumes_BrandId")]
[Index("CountryCode", Name = "IX_Perfumes_CountryCode")]
[Index("Name", Name = "IX_Perfumes_Name")]
public partial class Perfume
{
    [Key]
    public int PerfumeId { get; set; }

    [StringLength(150)]
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
    public virtual ICollection<PerfumeListItem> PerfumeListItems { get; set; } = new List<PerfumeListItem>();

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeLongevityVote> PerfumeLongevityVotes { get; set; } = new List<PerfumeLongevityVote>();

    [InverseProperty("Perfume")]
    public virtual ICollection<PerfumeNote> PerfumeNotes { get; set; } = new List<PerfumeNote>();

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
}
