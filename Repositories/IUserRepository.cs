using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public interface IUserRepository
    {
        Task<string> Create(IdentityUser user);
        Task<IdentityUser?> GetByEmail(string email);
        Task<IdentityUser?> GetByUserName(string normalizedUserName);
        Task<IdentityUser?> GetById(string id);
        Task AssignClaims(IdentityUser user, IEnumerable<Claim> claims);
        Task<IList<Claim>> GetClaims(IdentityUser user);
        Task RemoveClaims(IdentityUser user, IEnumerable<Claim> claims);
    }
}
