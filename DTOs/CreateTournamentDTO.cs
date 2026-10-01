namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class CreateTournamentDTO
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public decimal? EntryFee { get; set; }
        public int? MaxPlayers { get; set; }
        public string? Status { get; set; }
    }
}