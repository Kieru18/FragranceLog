using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_Notes_Name", IsUnique = true)]
public partial class Note
{
    [Key]
    public int NoteId { get; set; }

    [StringLength(40)]
    public string Name { get; set; } = null!;

    [InverseProperty("Note")]
    public virtual NotePhoto? NotePhoto { get; set; }

    [InverseProperty("Note")]
    public virtual ICollection<PerfumeNote> PerfumeNotes { get; set; } = new List<PerfumeNote>();
}
