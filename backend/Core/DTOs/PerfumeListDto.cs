namespace Core.Dtos;

public class PerfumeListDto
{
    public int PerfumeListId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsSystem { get; set; }
    public DateTime CreationDate { get; set; }
}
