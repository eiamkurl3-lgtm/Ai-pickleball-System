using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Entities;
using AI_Based_Pickleball_Tournament_Management.Fitlers;
using AI_Based_Pickleball_Tournament_Management.Repositories;

namespace AI_Based_Pickleball_Tournament_Management.Endpoints
{
    public static class MatchEndpoints
    {
        public static RouteGroupBuilder MapMatchEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllMatches)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("matches-get"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapGet("/tournament/{tournamentId:int}", GetMatchesByTournament)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("matches-by-tournament"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapGet("/{id:int}", GetMatchById)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("match-get-by-id"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapPost("/", CreateMatch)
                //.RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<CreateMatchDTO>>()
                .DisableAntiforgery();

            group.MapPut("/{id:int}", UpdateMatch)
                //.RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<UpdateMatchDTO>>()
                .DisableAntiforgery();

            group.MapDelete("/{id:int}", DeleteMatch);
                //.RequireAuthorization("isadmin");

            return group;
        }

        static async Task<Ok<List<MatchDTO>>> GetAllMatches(IMatchRepository repository, IMapper mapper, [AsParameters] PaginationDTO pagination)
        {
            var matches = await repository.GetAll(pagination);
            var matchDTOs = mapper.Map<List<MatchDTO>>(matches);
            return TypedResults.Ok(matchDTOs);
        }

        static async Task<Results<Ok<List<MatchDTO>>, NotFound>> GetMatchesByTournament(int tournamentId,
            IMatchRepository repository, ITournamentRepository tournamentRepository, IMapper mapper,
            [AsParameters] PaginationDTO pagination)
        {
            if (!await tournamentRepository.Exists(tournamentId))
            {
                return TypedResults.NotFound();
            }

            var matches = await repository.GetByTournament(tournamentId, pagination);
            var matchDTOs = mapper.Map<List<MatchDTO>>(matches);
            return TypedResults.Ok(matchDTOs);
        }

        static async Task<Results<Ok<MatchDTO>, NotFound>> GetMatchById(int id, IMatchRepository repository, IMapper mapper)
        {
            var match = await repository.GetById(id);

            if (match is null)
            {
                return TypedResults.NotFound();
            }

            var matchDTO = mapper.Map<MatchDTO>(match);
            return TypedResults.Ok(matchDTO);
        }

        static async Task<Results<Created<MatchDTO>, ValidationProblem>> CreateMatch(CreateMatchDTO createMatchDTO,
            IOutputCacheStore outputCacheStore, IMatchRepository repository, ITournamentRepository tournamentRepository,
            UserManager<IdentityUser> userManager, IMapper mapper)
        {
            var (playerIds, referenceErrors) = await ResolvePlayerIds(createMatchDTO, tournamentRepository, userManager);

            if (referenceErrors is not null)
            {
                return TypedResults.ValidationProblem(referenceErrors);
            }

            var match = mapper.Map<Match>(createMatchDTO);
            match.Player1Id = playerIds.Player1Id;
            match.Player2Id = playerIds.Player2Id;
            var id = await repository.Create(match);
            await EvictMatchCaches(outputCacheStore);

            var created = await repository.GetById(id);
            var matchDTO = mapper.Map<MatchDTO>(created);
            return TypedResults.Created($"/Match/{id}", matchDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateMatch(int id,
            UpdateMatchDTO updateMatchDTO, IMatchRepository repository, IOutputCacheStore outputCacheStore,
            IMapper mapper)
        {
            var matchDB = await repository.GetById(id);

            if (matchDB is null)
            {
                return TypedResults.NotFound();
            }

            // The payload only carries the result, so scheduling, opponents and the
            // legacy court/winner columns are carried over from the stored match.
            var matchForUpdate = mapper.Map<Match>(updateMatchDTO);
            matchForUpdate.Id = id;
            matchForUpdate.TournamentId = matchDB.TournamentId;
            matchForUpdate.ScheduledStart = matchDB.ScheduledStart;
            matchForUpdate.Player1Id = matchDB.Player1Id;
            matchForUpdate.Player2Id = matchDB.Player2Id;
            matchForUpdate.Court = matchDB.Court;
            matchForUpdate.WinnerSide = matchDB.WinnerSide;

            await repository.Update(matchForUpdate);
            await EvictMatchCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteMatch(int id,
            IMatchRepository repository, IOutputCacheStore outputCacheStore)
        {
            var matchDB = await repository.GetById(id);

            if (matchDB is null)
            {
                return TypedResults.NotFound();
            }

            await repository.Delete(id);
            await EvictMatchCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        private static async Task EvictMatchCaches(IOutputCacheStore outputCacheStore)
        {
            await outputCacheStore.EvictByTagAsync("matches-get", default);
            await outputCacheStore.EvictByTagAsync("match-get-by-id", default);
            await outputCacheStore.EvictByTagAsync("matches-by-tournament", default);
        }

        // A match must point at a real tournament, and both opponents are identified by
        // username: the player ids are resolved from those names, never read off the body.
        private static async Task<((string Player1Id, string Player2Id) PlayerIds, Dictionary<string, string[]>? Errors)> ResolvePlayerIds(
            CreateMatchDTO matchDTO, ITournamentRepository tournamentRepository, UserManager<IdentityUser> userManager)
        {
            var errors = new Dictionary<string, string[]>();

            if (!await tournamentRepository.Exists(matchDTO.TournamentId))
            {
                errors["TournamentId"] = new[] { $"No tournament exists with id {matchDTO.TournamentId}" };
            }

            var player1 = await userManager.FindByNameAsync(matchDTO.UserName);

            if (player1 is null)
            {
                errors["UserName"] = new[] { $"No user exists with username {matchDTO.UserName}" };
            }

            var player2 = await userManager.FindByNameAsync(matchDTO.UserName2);

            if (player2 is null)
            {
                errors["UserName2"] = new[] { $"No user exists with username {matchDTO.UserName2}" };
            }

            if (errors.Count > 0)
            {
                return ((string.Empty, string.Empty), errors);
            }

            return ((player1!.Id, player2!.Id), null);
        }
    }
}
