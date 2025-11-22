using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Index("Name", Name = "UQ_NoteTypes_Name", IsUnique = true)]
public partial class NoteType
{
    [Key]
    public int NoteTypeId { get; set; }

    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("NoteType")]
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
