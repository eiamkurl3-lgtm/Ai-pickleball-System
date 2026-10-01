using Microsoft.AspNetCore.Identity;

namespace AI_Based_Pickleball_Tournament_Management.Services
{
    public interface IUsersService
    {
        Task<IdentityUser?> GetUser();
    }
}
