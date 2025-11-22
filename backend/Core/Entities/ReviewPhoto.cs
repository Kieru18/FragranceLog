using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

public partial class ReviewPhoto
{
    [Key]
    public int PhotoId { get; set; }

    [StringLength(500)]
    public string Path { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UploadDate { get; set; }

    public int? ReviewId { get; set; }

    [ForeignKey("ReviewId")]
    [InverseProperty("ReviewPhotos")]
    public virtual Review? Review { get; set; }
}
