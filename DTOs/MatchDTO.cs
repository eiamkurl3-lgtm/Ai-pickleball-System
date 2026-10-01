namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class MatchDTO
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }

        public string UserName { get; set; } = null!;
        public string UserName2 { get; set; } = null!;

        public DateTime ScheduledStart { get; set; }
        public string? Status { get; set; }
        public int SideAScore { get; set; }
        public int SideBScore { get; set; }
    }
}
