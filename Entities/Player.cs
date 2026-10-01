namespace AI_Based_Pickleball_Tournament_Management.Entities
{
    public class Player
    {
        public int Id { get; set; }

        // Identity user this profile belongs to (Users.Id), one profile per user
        public string UserId { get; set; } = null!;

        public string UserName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public decimal? SkillLevel { get; set; }
        public string? Status { get; set; }

        // Many-to-many navigation property
        public List<PlayerTournament> PlayerTournaments { get; set; } = new List<PlayerTournament>();
    }
}
