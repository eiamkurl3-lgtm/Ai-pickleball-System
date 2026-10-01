using AutoMapper;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;

namespace AI_Based_Pickleball_Tournament_Management.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CreateTournamentDTO, Tournament>();
            CreateMap<Tournament, TournamentDTO>();

            CreateMap<CreateMatchDTO, Match>();
            CreateMap<Match, MatchDTO>();

            CreateMap<CreatePlayerDTO, Player>();
            CreateMap<UpdatePlayerDTO, Player>();
            CreateMap<Player, PlayerDTO>();
        }
    }
}