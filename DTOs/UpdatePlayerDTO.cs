namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    // Update payload is deliberately narrow: a profile's identity (UserId / UserName)
    // and its date of birth are set at create time and are not editable here.
    public class UpdatePlayerDTO
    {
        public decimal? SkillLevel { get; set; }
        public string? Status { get; set; }
    }
}
