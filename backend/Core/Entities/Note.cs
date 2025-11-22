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

    public int NoteTypeId { get; set; }

    [InverseProperty("Note")]
    public virtual ICollection<NotePhoto> NotePhotos { get; set; } = new List<NotePhoto>();

    [ForeignKey("NoteTypeId")]
    [InverseProperty("Notes")]
    public virtual NoteType NoteType { get; set; } = null!;

    [ForeignKey("NoteId")]
    [InverseProperty("Notes")]
    public virtual ICollection<Perfume> Perfumes { get; set; } = new List<Perfume>();
}
