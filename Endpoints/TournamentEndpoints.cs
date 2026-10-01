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
                .AddEndpointFilter<ValidationFilter<UpdateTournamentDTO>>()
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
            await EvictTournamentCaches(outputCacheStore);

            var tournamentDTO = mapper.Map<TournamentDTO>(tournament);
            return TypedResults.Created($"/Tournament/{id}", tournamentDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateTournament(int id,
            UpdateTournamentDTO updateTournamentDTO, ITournamentRepository repository,
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var tournamentDB = await repository.GetById(id);

            if (tournamentDB is null)
            {
                return TypedResults.NotFound();
            }

            // The payload only carries the status, so the name and scheduling details
            // are carried over from the stored tournament.
            var tournamentForUpdate = mapper.Map<Tournament>(updateTournamentDTO);
            tournamentForUpdate.Id = id;
            tournamentForUpdate.Name = tournamentDB.Name;
            tournamentForUpdate.StartDate = tournamentDB.StartDate;
            tournamentForUpdate.EndDate = tournamentDB.EndDate;
            tournamentForUpdate.Location = tournamentDB.Location;
            tournamentForUpdate.EntryFee = tournamentDB.EntryFee;
            tournamentForUpdate.MaxPlayers = tournamentDB.MaxPlayers;

            await repository.Update(tournamentForUpdate);
            await EvictTournamentCaches(outputCacheStore);
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
            await EvictTournamentCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        // Every tag this group declares must go, otherwise GET /Tournament/{id}
        // keeps serving a stale copy for the remainder of its 60s window.
        private static async Task EvictTournamentCaches(IOutputCacheStore outputCacheStore)
        {
            await outputCacheStore.EvictByTagAsync("tournaments-get", default);
            await outputCacheStore.EvictByTagAsync("tournament-get-by-id", default);
        }
    }
}