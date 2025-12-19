using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Genders_Name", IsUnique = true)]
public partial class Gender
{
    [Key]
    public int GenderId { get; set; }

    [StringLength(6)]
    public string Name { get; set; } = null!;

    [InverseProperty("Gender")]
    public virtual ICollection<PerfumeGenderVote> PerfumeGenderVotes { get; set; } = new List<PerfumeGenderVote>();
}
