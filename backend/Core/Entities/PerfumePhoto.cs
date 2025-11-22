using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

public partial class PerfumePhoto
{
    [Key]
    public int PhotoId { get; set; }

    [StringLength(500)]
    public string Path { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UploadDate { get; set; }

    public int PerfumeId { get; set; }

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumePhotos")]
    public virtual Perfume Perfume { get; set; } = null!;
}
