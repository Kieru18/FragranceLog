namespace Core.DTOs
{
    public sealed class SharedListDto
    {
        public Guid ShareToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
