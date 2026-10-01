using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;
using AI_Based_Pickleball_Tournament_Management.Fitlers;
using AI_Based_Pickleball_Tournament_Management.Repositories;

namespace AI_Based_Pickleball_Tournament_Management.Endpoints
{
    public static class TournamentEndpoints
    {
        public static RouteGroupBuilder MapTournamentEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllTournaments)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("tournaments-get"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapGet("/{id:int}", GetTournamentById)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("tournament-get-by-id"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapPost("/", CreateTournament)
                //.RequireAuthorization("isadmin")
                .DisableAntiforgery();

            group.MapPut("/{id:int}", UpdateTournament)
                //.RequireAuthorization("isadmin")
                .DisableAntiforgery();

            group.MapDelete("/{id:int}", DeleteTournament);
                //.RequireAuthorization("isadmin");

            return group;
        }

        static async Task<Ok<List<TournamentDTO>>> GetAllTournaments(ITournamentRepository repository, IMapper mapper, [AsParameters] PaginationDTO pagination)
        {
            var tournaments = await repository.GetAll(pagination);
            var tournamentDTOs = mapper.Map<List<TournamentDTO>>(tournaments);
            return TypedResults.Ok(tournamentDTOs);
        }

        static async Task<Results<Ok<TournamentDTO>, NotFound>> GetTournamentById(int id, ITournamentRepository repository, IMapper mapper)
        {
            var tournament = await repository.GetById(id);

            if (tournament is null)
            {
                return TypedResults.NotFound();
            }

            var tournamentDTO = mapper.Map<TournamentDTO>(tournament);
            return TypedResults.Ok(tournamentDTO);
        }

        static async Task<Created<TournamentDTO>> CreateTournament(CreateTournamentDTO createTournamentDTO,
            IOutputCacheStore outputCacheStore, ITournamentRepository repository, IMapper mapper)
        {
            var tournament = mapper.Map<Tournament>(createTournamentDTO);
            var id = await repository.Create(tournament);
            await outputCacheStore.EvictByTagAsync("tournaments-get", default);

            var tournamentDTO = mapper.Map<TournamentDTO>(tournament);
            return TypedResults.Created($"/Tournament/{id}", tournamentDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateTournament(int id,
            CreateTournamentDTO updateTournamentDTO, ITournamentRepository repository,
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var tournamentDB = await repository.GetById(id);

            if (tournamentDB is null)
            {
                return TypedResults.NotFound();
            }

            var tournamentForUpdate = mapper.Map<Tournament>(updateTournamentDTO);
            tournamentForUpdate.Id = id;

            await repository.Update(tournamentForUpdate);
            await outputCacheStore.EvictByTagAsync("tournaments-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteTournament(int id,
            ITournamentRepository repository, IOutputCacheStore outputCacheStore)
        {
            var tournamentDB = await repository.GetById(id);

            if (tournamentDB is null)
            {
                return TypedResults.NotFound();
            }

            await repository.Delete(id);
            await outputCacheStore.EvictByTagAsync("tournaments-get", default);
            return TypedResults.NoContent();
        }
    }
}