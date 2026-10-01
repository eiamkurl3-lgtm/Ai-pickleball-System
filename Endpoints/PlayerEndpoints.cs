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
    public static class PlayerEndpoints
    {
        public static RouteGroupBuilder MapPlayerEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllPlayers)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("players-get"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapGet("/user/{userId}", GetPlayerByUserId)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("player-get-by-user"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapGet("/{id:int}", GetPlayerById)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("player-get-by-id"))
                .AddEndpointFilter(new ClientCacheFilter(30));

            group.MapPost("/", CreatePlayer)
                .RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<CreatePlayerDTO>>()
                .DisableAntiforgery();

            group.MapPut("/{id:int}", UpdatePlayer)
                .RequireAuthorization("isadmin")
                .AddEndpointFilter(new ValidationFilter<UpdatePlayerDTO>())
                .DisableAntiforgery();

            group.MapDelete("/{id:int}", DeletePlayer)
                .RequireAuthorization("isadmin");

            // Rewrites the whole registration set for the player, hence admin only.
            group.MapPost("/{id:int}/assign-tournaments", AssignTournaments)
                //.RequireAuthorization("isadmin")
                .DisableAntiforgery();

            return group;
        }

        static async Task<Ok<List<PlayerDTO>>> GetAllPlayers(IPlayerRepository repository, IMapper mapper,
            [AsParameters] PaginationDTO pagination)
        {
            var players = await repository.GetAll(pagination);
            var playerDTOs = mapper.Map<List<PlayerDTO>>(players);
            return TypedResults.Ok(playerDTOs);
        }

        static async Task<Results<Ok<PlayerDTO>, NotFound>> GetPlayerById(int id,
            IPlayerRepository repository, IMapper mapper)
        {
            var player = await repository.GetById(id);

            if (player is null)
            {
                return TypedResults.NotFound();
            }

            var playerDTO = mapper.Map<PlayerDTO>(player);
            return TypedResults.Ok(playerDTO);
        }

        static async Task<Results<Ok<PlayerDTO>, NotFound>> GetPlayerByUserId(string userId,
            IPlayerRepository repository, IMapper mapper)
        {
            var player = await repository.GetByUserId(userId);

            if (player is null)
            {
                return TypedResults.NotFound();
            }

            var playerDTO = mapper.Map<PlayerDTO>(player);
            return TypedResults.Ok(playerDTO);
        }

        static async Task<Results<Created<PlayerDTO>, ValidationProblem>> CreatePlayer(CreatePlayerDTO createPlayerDTO,
            IOutputCacheStore outputCacheStore, IPlayerRepository repository, UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            var (user, profileErrors) = await FindProfile(createPlayerDTO, repository, userManager);

            if (profileErrors.Count > 0)
            {
                return TypedResults.ValidationProblem(profileErrors);
            }

            var player = mapper.Map<Player>(createPlayerDTO);
            player.UserId = user!.Id;
            var id = await repository.Create(player);
            await EvictPlayerCaches(outputCacheStore);

            var created = await repository.GetById(id);
            var playerDTO = mapper.Map<PlayerDTO>(created);
            return TypedResults.Created($"/Player/{id}", playerDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdatePlayer(int id,
            UpdatePlayerDTO updatePlayerDTO, IPlayerRepository repository,
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var playerDB = await repository.GetById(id);

            if (playerDB is null)
            {
                return TypedResults.NotFound();
            }

            // Identity columns stay as stored: only the mutable fields are patched, which
            // keeps UQ_Players_UserId out of reach on this route.
            var playerForUpdate = mapper.Map<Player>(updatePlayerDTO);
            playerForUpdate.Id = id;
            playerForUpdate.UserId = playerDB.UserId;
            playerForUpdate.UserName = playerDB.UserName;
            playerForUpdate.DateOfBirth = playerDB.DateOfBirth;

            await repository.Update(playerForUpdate);
            await EvictPlayerCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeletePlayer(int id,
            IPlayerRepository repository, IOutputCacheStore outputCacheStore)
        {
            var playerDB = await repository.GetById(id);

            if (playerDB is null)
            {
                return TypedResults.NotFound();
            }

            await repository.Delete(id);
            await EvictPlayerCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest>> AssignTournaments(int id,
            List<int> tournamentIds, IPlayerRepository playerRepository, ITournamentRepository tournamentRepository,
            IOutputCacheStore outputCacheStore)
        {
            if (!await playerRepository.Exists(id))
            {
                return TypedResults.NotFound();
            }

            var existingIds = await tournamentRepository.Exists(tournamentIds);
            if (existingIds.Count != tournamentIds.Count)
            {
                return TypedResults.BadRequest();
            }

            await playerRepository.AssignTournaments(id, tournamentIds);
            await EvictPlayerCaches(outputCacheStore);
            return TypedResults.NoContent();
        }

        private static async Task EvictPlayerCaches(IOutputCacheStore outputCacheStore)
        {
            await outputCacheStore.EvictByTagAsync("players-get", default);
            await outputCacheStore.EvictByTagAsync("player-get-by-id", default);
            await outputCacheStore.EvictByTagAsync("player-get-by-user", default);
        }

        // A player profile must point at a real user and there can only be one per user.
        // The UserId is never taken from the body: it is resolved from the supplied UserName.
        private static async Task<(IdentityUser? User, Dictionary<string, string[]> Errors)> FindProfile(
            CreatePlayerDTO playerDTO, IPlayerRepository repository, UserManager<IdentityUser> userManager)
        {
            var errors = new Dictionary<string, string[]>();

            var user = await userManager.FindByNameAsync(playerDTO.UserName);

            if (user is null)
            {
                errors["UserName"] = new[] { $"No user exists with username {playerDTO.UserName}" };
                return (null, errors);
            }

            var existingPlayer = await repository.GetByUserId(user.Id);

            if (existingPlayer is not null)
            {
                errors["UserName"] = new[] { "A player profile already exists for this user" };
            }

            return (user, errors);
        }
    }
}
