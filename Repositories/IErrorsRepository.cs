using AI_Based_Pickleball_Tournament_Management.Entities;

namespace AI_Based_Pickleball_Tournament_Management.Repositories
{
    public interface IErrorsRepository
    {
        Task<Guid> Create(Error error);
    }
}