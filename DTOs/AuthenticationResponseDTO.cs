namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    public class AuthenticationResponseDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}
