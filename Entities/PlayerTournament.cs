namespace AI_Based_Pickleball_Tournament_Management.Entities
{
    public class PlayerTournament
    {
        public int PlayerId { get; set; }
        public int TournamentId { get; set; }
        public DateTime RegisteredAt { get; set; }

        public Player Player { get; set; } = null!;
        public Tournament Tournament { get; set; } = null!;
    }
}
