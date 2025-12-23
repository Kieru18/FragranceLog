using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Table("SharedList")]
[Index("ShareToken", Name = "UQ_SharedList_ShareToken", IsUnique = true)]
public partial class SharedList
{
    [Key]
    public int SharedListId { get; set; }

    public int PerfumeListId { get; set; }

    public int OwnerUserId { get; set; }

    public Guid ShareToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpirationDate { get; set; }

    [ForeignKey("PerfumeListId")]
    [InverseProperty("SharedList")]
    public virtual PerfumeList PerfumeList { get; set; } = null!;
}
