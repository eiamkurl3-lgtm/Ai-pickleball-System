using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public interface IPlayerRepository
    {
        Task<int> Create(Player player);
        Task<List<Player>> GetAll(PaginationDTO pagination);
        Task<Player?> GetById(int id);
        Task<Player?> GetByUserId(string userId);
        Task<bool> Exists(int id);
        Task Update(Player player);
        Task Delete(int id);
        Task AssignTournaments(int id, List<int> tournamentIds);
    }
}
