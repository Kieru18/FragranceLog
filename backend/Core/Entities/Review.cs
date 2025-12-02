using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

public partial class Review
{
    [Key]
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int PerfumeId { get; set; }

    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ReviewDate { get; set; }

    [ForeignKey("PerfumeId")]
    [InverseProperty("Reviews")]
    public virtual Perfume Perfume { get; set; } = null!;

    [InverseProperty("Review")]
    public virtual ReviewPhoto? ReviewPhoto { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Reviews")]
    public virtual User User { get; set; } = null!;
}
