using Dapper;
using Microsoft.Data.SqlClient;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;
using System.Data;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public PlayerRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> Create(Player player)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>("Create_Player", new
            {
                player.UserId,
                player.UserName,
                player.DateOfBirth,
                player.SkillLevel,
                player.Status
            }, commandType: CommandType.StoredProcedure);

            player.Id = id;
            return id;
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>("Player_IfExists", new { Id = id }, commandType: CommandType.StoredProcedure);
            return exists;
        }

        public async Task<List<Player>> GetAll(PaginationDTO pagination)
        {
            using var connection = new SqlConnection(connectionString);

            var players = await connection.QueryAsync<Player>("Player_GetAll", new
            {
                Page = PaginationDTO.ResolvePage(pagination.Page),
                RecordsPerPage = PaginationDTO.ResolveRecordsPerPage(pagination.RecordsPerPage)
            }, commandType: CommandType.StoredProcedure);

            var playerCount = await connection.QuerySingleAsync<int>("Player_Count", commandType: CommandType.StoredProcedure);

            httpContext.Response.Headers.Append("totalAmountOfRecords", playerCount.ToString());

            return players.ToList();
        }

        public async Task<Player?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Player>("Player_GetByID", new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Player?> GetByUserId(string userId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Player>("Player_GetByUserId", new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task Update(Player player)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Update_Player", new
            {
                player.Id,
                player.UserId,
                player.UserName,
                player.DateOfBirth,
                player.SkillLevel,
                player.Status
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Player_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task AssignTournaments(int id, List<int> tournamentIds)
        {
            using var connection = new SqlConnection(connectionString);

            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            foreach (var tournamentId in tournamentIds)
            {
                dt.Rows.Add(tournamentId);
            }

            // RegisteredAt is stamped by the stored procedure, so only the ids travel here.
            await connection.ExecuteAsync("Player_AssignTournaments", new
            {
                PlayerId = id,
                TournamentIds = dt.AsTableValuedParameter("dbo.IntArray")
            }, commandType: CommandType.StoredProcedure);
        }
    }
}
