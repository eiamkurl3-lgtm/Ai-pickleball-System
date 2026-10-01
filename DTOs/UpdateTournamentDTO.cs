namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    // Update payload is deliberately narrow: a tournament's identity (Name) and its
    // scheduling details (StartDate, EndDate, Location, EntryFee, MaxPlayers) are set
    // at create time and are not editable here. Only the lifecycle status can change.
    public class UpdateTournamentDTO
    {
        public string Status { get; set; } = null!;
    }
}
