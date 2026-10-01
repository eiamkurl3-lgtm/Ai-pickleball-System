using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public interface IMatchRepository
    {
        Task<int> Create(Match match);
        Task<List<Match>> GetAll(PaginationDTO pagination);
        Task<List<Match>> GetByTournament(int tournamentId, PaginationDTO pagination);
        Task<Match?> GetById(int id);
        Task<bool> Exists(int id);
        Task Update(Match match);
        Task Delete(int id);
    }
}
