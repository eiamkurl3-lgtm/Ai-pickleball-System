namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class CreateMatchDTO
    {
        public int TournamentId { get; set; }

        // Usernames of the two opponents. The user ids are never taken from the
        // body: they are resolved from these names when the match is created.
        public string UserName { get; set; } = null!;
        public string UserName2 { get; set; } = null!;

        public DateTime ScheduledStart { get; set; }
        public string Status { get; set; } = null!;
        public int SideAScore { get; set; }
        public int SideBScore { get; set; }
    }
}
