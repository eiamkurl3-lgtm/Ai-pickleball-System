using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public interface ITournamentRepository
    {
        Task<int> Create(Tournament tournament);
        Task<List<Tournament>> GetAll(PaginationDTO pagination);
        Task<Tournament?> GetById(int id);
        Task<bool> Exists(int id);
        Task<List<int>> Exists(List<int> ids);
        Task Update(Tournament tournament);
        Task Delete(int id);
    }
}