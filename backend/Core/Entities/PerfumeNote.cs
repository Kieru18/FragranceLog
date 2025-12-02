using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey("PerfumeId", "NoteId", "NoteTypeId")]
[Table("PerfumeNote")]
public partial class PerfumeNote
{
    [Key]
    public int PerfumeId { get; set; }

    [Key]
    public int NoteId { get; set; }

    [Key]
    public int NoteTypeId { get; set; }

    [ForeignKey("NoteId")]
    [InverseProperty("PerfumeNotes")]
    public virtual Note Note { get; set; } = null!;

    [ForeignKey("NoteTypeId")]
    [InverseProperty("PerfumeNotes")]
    public virtual NoteType NoteType { get; set; } = null!;

    [ForeignKey("PerfumeId")]
    [InverseProperty("PerfumeNotes")]
    public virtual Perfume Perfume { get; set; } = null!;
}
