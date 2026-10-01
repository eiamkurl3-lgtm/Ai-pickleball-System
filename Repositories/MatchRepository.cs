using Dapper;
using Microsoft.Data.SqlClient;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;
using System.Data;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public MatchRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> Create(Match match)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>("Create_Match", new
            {
                match.TournamentId,
                match.Player1Id,
                match.Player2Id,
                match.Player3Id,
                match.Player4Id,
                match.ScheduledStart,
                match.Court,
                match.Status,
                match.SideAScore,
                match.SideBScore,
                match.WinnerSide
            }, commandType: CommandType.StoredProcedure);

            match.Id = id;
            return id;
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>("Match_IfExists", new { Id = id }, commandType: CommandType.StoredProcedure);
            return exists;
        }

        public async Task<List<Match>> GetAll(PaginationDTO pagination)
        {
            using var connection = new SqlConnection(connectionString);

            var matches = await connection.QueryAsync<Match>("Match_GetAll", new
            {
                Page = PaginationDTO.ResolvePage(pagination.Page),
                RecordsPerPage = PaginationDTO.ResolveRecordsPerPage(pagination.RecordsPerPage)
            }, commandType: CommandType.StoredProcedure);

            var matchCount = await connection.QuerySingleAsync<int>("Match_Count", commandType: CommandType.StoredProcedure);

            httpContext.Response.Headers.Append("totalAmountOfRecords", matchCount.ToString());

            return matches.ToList();
        }

        public async Task<List<Match>> GetByTournament(int tournamentId, PaginationDTO pagination)
        {
            using var connection = new SqlConnection(connectionString);

            var matches = await connection.QueryAsync<Match>("Match_GetByTournament", new
            {
                TournamentId = tournamentId,
                Page = PaginationDTO.ResolvePage(pagination.Page),
                RecordsPerPage = PaginationDTO.ResolveRecordsPerPage(pagination.RecordsPerPage)
            }, commandType: CommandType.StoredProcedure);

            var matchCount = await connection.QuerySingleAsync<int>("Match_CountByTournament",
                new { TournamentId = tournamentId }, commandType: CommandType.StoredProcedure);

            httpContext.Response.Headers.Append("totalAmountOfRecords", matchCount.ToString());

            return matches.ToList();
        }

        public async Task<Match?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Match>("Match_GetByID", new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task Update(Match match)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Update_Match", new
            {
                match.Id,
                match.TournamentId,
                match.Player1Id,
                match.Player2Id,
                match.Player3Id,
                match.Player4Id,
                match.ScheduledStart,
                match.Court,
                match.Status,
                match.SideAScore,
                match.SideBScore,
                match.WinnerSide
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Match_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
    }
}
