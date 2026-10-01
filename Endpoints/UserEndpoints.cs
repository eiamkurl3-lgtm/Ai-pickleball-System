using AI_Based_Pickleball_Tournament_Management.DTOs;
using AI_Based_Pickleball_Tournament_Management.Fitlers;
using AI_Based_Pickleball_Tournament_Management.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AI_Based_Pickleball_Tournament_Management.Endpoints
{
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/register", Register)
                .AllowAnonymous()
                .AddEndpointFilter<ValidationFilter<RegisterUserDTO>>();
            group.MapPost("/login", Login)
                .AllowAnonymous()
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
            group.MapPost("/MakeAdmin", MakeAdmin)
                .RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>>();
            group.MapPost("/RemoveAdmin", RemoveAdmin)
                .RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>>();
            return group;
        }

        private static async Task<AuthenticationResponseDTO> BuildToken(
            IdentityUser user,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim("username", user.UserName!),
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim("email", user.Email));
            }

            var claimsFromDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsFromDB);

            var key = KeysHandler.GetKey(configuration).First();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }

        private static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<IEnumerable<IdentityError>>>> Register(
            RegisterUserDTO registerUserDTO,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager)
        {
            var user = new IdentityUser
            {
                UserName = registerUserDTO.UserName
            };

            var result = await userManager.CreateAsync(user, registerUserDTO.Password);

            if (result.Succeeded)
            {
                return TypedResults.Ok(await BuildToken(user, configuration, userManager));
            }
            else
            {
                return TypedResults.BadRequest(result.Errors);
            }
        }

        private static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(
            UserCredentialsDTO userCredentials,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            var user = await userManager.FindByNameAsync(userCredentials.UserName);

            if (user is null)
            {
                return TypedResults.BadRequest("There was a problem with the username or the password");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return TypedResults.Ok(await BuildToken(user, configuration, userManager));
            }
            else
            {
                return TypedResults.BadRequest("There was a problem with the username or the password");
            }
        }

        private static async Task<Results<NoContent, NotFound>> MakeAdmin(
            EditClaimDTO editClaimDTO,
            UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByNameAsync(editClaimDTO.UserName);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }

        private static async Task<Results<NoContent, NotFound>> RemoveAdmin(
            EditClaimDTO editClaimDTO,
            UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByNameAsync(editClaimDTO.UserName);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }
    }
}
