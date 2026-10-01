namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class CreateMatchDTO
    {
        public int TournamentId { get; set; }

        public string Player1Id { get; set; } = null!;
        public string Player2Id { get; set; } = null!;
        public string? Player3Id { get; set; }
        public string? Player4Id { get; set; }

        public DateTime ScheduledStart { get; set; }
        public string? Court { get; set; }
        public string? Status { get; set; }
        public int? SideAScore { get; set; }
        public int? SideBScore { get; set; }
        public string? WinnerSide { get; set; }
    }
}
