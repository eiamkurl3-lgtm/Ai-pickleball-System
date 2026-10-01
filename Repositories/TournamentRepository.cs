using Dapper;
using Microsoft.Data.SqlClient;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;
using System.Data;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public TournamentRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> Create(Tournament tournament)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>("Create_Tournament", new
            {
                tournament.Name,
                tournament.StartDate,
                tournament.EndDate,
                tournament.Location,
                tournament.EntryFee,
                tournament.MaxPlayers,
                tournament.Status
            }, commandType: CommandType.StoredProcedure);

            tournament.Id = id;
            return id;
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>("Tournament_IfExists", new { Id = id }, commandType: CommandType.StoredProcedure);
            return exists;
        }

        public async Task<List<int>> Exists(List<int> ids)
        {
            using var connection = new SqlConnection(connectionString);

            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            foreach (var id in ids)
            {
                dt.Rows.Add(id);
            }

            var existingIds = await connection.QueryAsync<int>("Tournament_GetBySeveralIds",
                new { TournamentIds = dt.AsTableValuedParameter("dbo.IntArray") }, commandType: CommandType.StoredProcedure);

            return existingIds.ToList();
        }

        public async Task<List<Tournament>> GetAll(PaginationDTO pagination)
        {
            using var connection = new SqlConnection(connectionString);

            var tournaments = await connection.QueryAsync<Tournament>("Tournament_GetAll", new
            {
                Page = PaginationDTO.ResolvePage(pagination.Page),
                RecordsPerPage = PaginationDTO.ResolveRecordsPerPage(pagination.RecordsPerPage)
            }, commandType: CommandType.StoredProcedure);

            var tournamentCount = await connection.QuerySingleAsync<int>("Tournament_Count", commandType: CommandType.StoredProcedure);

            httpContext.Response.Headers.Append("totalAmountOfRecords", tournamentCount.ToString());

            return tournaments.ToList();
        }

        public async Task<Tournament?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Tournament>("Tournament_GetByID", new { Id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task Update(Tournament tournament)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Update_Tournament", new
            {
                tournament.Id,
                tournament.Name,
                tournament.StartDate,
                tournament.EndDate,
                tournament.Location,
                tournament.EntryFee,
                tournament.MaxPlayers,
                tournament.Status
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Tournament_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
    }
}