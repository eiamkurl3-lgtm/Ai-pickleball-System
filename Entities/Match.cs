namespace AI_Based_Pickleball_Tournament_Management.Entities
{
    public class Match
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }

        // Side A is Player1, Side B is Player2. The ids are resolved from the
        // usernames supplied on the request and are never taken from the body.
        public string Player1Id { get; set; } = null!;
        public string Player2Id { get; set; } = null!;

        // Read-only projection of the joined Users rows.
        public string? Player1UserName { get; set; }
        public string? Player2UserName { get; set; }

        public DateTime ScheduledStart { get; set; }
        public string? Status { get; set; }

        // Aggregate points won by each side across the whole match.
        public int SideAScore { get; set; }
        public int SideBScore { get; set; }

        // Court and winner are no longer part of the create payload. The columns
        // stay so historical rows keep their data; the endpoints never accept them.
        public string? Court { get; set; }
        public string? WinnerSide { get; set; }
    }
}
