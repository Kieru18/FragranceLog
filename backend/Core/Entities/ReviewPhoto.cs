using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("ReviewId", Name = "UQ_ReviewPhotos_Review", IsUnique = true)]
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
    [InverseProperty("ReviewPhoto")]
    public virtual Review? Review { get; set; }
}
