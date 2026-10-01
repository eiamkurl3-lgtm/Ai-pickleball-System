namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    // Update payload is deliberately narrow: a match's scheduling (TournamentId,
    // ScheduledStart) and its two opponents are set at create time and are not
    // editable here. Only the result of the match can be revised.
    public class UpdateMatchDTO
    {
        public string Status { get; set; } = null!;
        public int SideAScore { get; set; }
        public int SideBScore { get; set; }
    }
}
