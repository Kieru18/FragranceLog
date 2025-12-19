using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Table("PerfumeListItem")]
[Index("PerfumeListId", "PerfumeId", Name = "UQ_PerfumeListItem_List_Perfume", IsUnique = true)]
public partial class PerfumeListItem
{
    [Key]
    public int PerfumeListItemId { get; set; }

    public int PerfumeListId { get; set; }

    public int PerfumeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeListItems")]
    public virtual Perfume Perfume { get; set; } = null!;

    [ForeignKey("PerfumeListId")]
    [InverseProperty("PerfumeListItems")]
    public virtual PerfumeList PerfumeList { get; set; } = null!;
}
