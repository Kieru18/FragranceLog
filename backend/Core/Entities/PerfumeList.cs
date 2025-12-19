using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[Table("PerfumeList")]
[Index("UserId", "Name", Name = "UQ_PerfumeList_User_Name", IsUnique = true)]
public partial class PerfumeList
{
    [Key]
    public int PerfumeListId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public bool IsSystem { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [InverseProperty("PerfumeList")]
    public virtual ICollection<PerfumeListItem> PerfumeListItems { get; set; } = new List<PerfumeListItem>();

    [ForeignKey("UserId")]
    [InverseProperty("PerfumeLists")]
    public virtual User User { get; set; } = null!;
}
