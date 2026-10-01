namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class PlayerDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;

        public string UserName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public decimal? SkillLevel { get; set; }
        public string? Status { get; set; }
    }
}
