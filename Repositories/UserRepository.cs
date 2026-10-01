using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<string> Create(IdentityUser user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                user.Id = Guid.NewGuid().ToString();
                await connection.ExecuteAsync("Users_Create",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.NormalizedEmail,
                        user.UserName,
                        user.NormalizedUserName,
                        user.PasswordHash
                    }, commandType: CommandType.StoredProcedure);

                return user.Id;
            }
        }

        public async Task<IdentityUser?> GetByEmail(string email)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection
                    .QuerySingleOrDefaultAsync<IdentityUser>("Users_GetByEmail", new { NormalizedEmail = email },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IdentityUser?> GetByUserName(string normalizedUserName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection
                    .QuerySingleOrDefaultAsync<IdentityUser>("Users_GetByUserName", new { NormalizedUserName = normalizedUserName },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IdentityUser?> GetById(string id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection
                    .QuerySingleOrDefaultAsync<IdentityUser>("Users_GetById", new { Id = id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IList<Claim>> GetClaims(IdentityUser user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var claims = await connection.QueryAsync<Claim>("Users_GetClaims",
                    new { user.Id }, commandType: CommandType.StoredProcedure);
                return claims.ToList();
            }
        }

        public async Task AssignClaims(IdentityUser user, IEnumerable<Claim> claims)
        {
            var sql = @"INSERT INTO UsersClaims (UserId, ClaimType, ClaimValue)
                                        VALUES (@Id, @Type, @Value)";
            var parameters = claims.Select(x => new { user.Id, x.Type, x.Value });

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, parameters);
            }
        }

        public async Task RemoveClaims(IdentityUser user, IEnumerable<Claim> claims)
        {
            var sql = "DELETE UsersClaims WHERE UserId = @Id AND ClaimType = @Type";
            var parameters = claims.Select(x => new { user.Id, x.Type });

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
