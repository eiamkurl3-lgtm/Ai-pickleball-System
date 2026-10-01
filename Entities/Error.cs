namespace AI_Based_Pickleball_Tournament_Management.Entities
{
    public class Error
    {
        public Guid Id { get; set; }
        public string ErrorMessage { get; set; } = null!;
        public string? StackTrace { get; set; }
        public DateTime Date { get; set; }
    }
}