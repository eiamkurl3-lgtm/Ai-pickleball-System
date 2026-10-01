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

            // Only the lifecycle status is editable, so every other column is
            // carried over from the stored tournament by the endpoint.
            CreateMap<UpdateTournamentDTO, Tournament>()
                .ForMember(t => t.Name, options => options.Ignore())
                .ForMember(t => t.StartDate, options => options.Ignore())
                .ForMember(t => t.EndDate, options => options.Ignore())
                .ForMember(t => t.Location, options => options.Ignore())
                .ForMember(t => t.EntryFee, options => options.Ignore())
                .ForMember(t => t.MaxPlayers, options => options.Ignore());

            // The player ids are resolved from UserName/UserName2 in the endpoint,
            // so neither maps from the request body.
            CreateMap<CreateMatchDTO, Match>()
                .ForMember(m => m.Player1Id, options => options.Ignore())
                .ForMember(m => m.Player2Id, options => options.Ignore());

            // Only the result is editable, so everything else is carried over
            // from the stored match by the endpoint.
            CreateMap<UpdateMatchDTO, Match>()
                .ForMember(m => m.Player1Id, options => options.Ignore())
                .ForMember(m => m.Player2Id, options => options.Ignore())
                .ForMember(m => m.TournamentId, options => options.Ignore())
                .ForMember(m => m.ScheduledStart, options => options.Ignore());

            CreateMap<Match, MatchDTO>()
                .ForMember(d => d.UserName, options => options.MapFrom(m => m.Player1UserName))
                .ForMember(d => d.UserName2, options => options.MapFrom(m => m.Player2UserName));

            CreateMap<CreatePlayerDTO, Player>();
            CreateMap<UpdatePlayerDTO, Player>();
            CreateMap<Player, PlayerDTO>();
        }
    }
}