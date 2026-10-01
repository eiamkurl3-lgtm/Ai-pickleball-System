using AI_Based_Pickleball_Tournament_Management.Entities;
using AI_Based_Pickleball_Tournament_Management.Endpoints;
using AI_Based_Pickleball_Tournament_Management.Repositories;
using AI_Based_Pickleball_Tournament_Management.Services;
using AI_Based_Pickleball_Tournament_Management.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IUserStore<IdentityUser>, UserStore>();
builder.Services.AddIdentityCore<IdentityUser>(o => o.User.RequireUniqueEmail = false);
builder.Services.AddTransient<SignInManager<IdentityUser>>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUsersService, UsersService>();

builder.Services.AddScoped<IErrorsRepository, ErrorsRepository>();

builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();

builder.Services.AddScoped<IMatchRepository, MatchRepository>();

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfiles>();
});

builder.Services.AddOutputCache();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKeys = KeysHandler.GetAllKeys(builder.Configuration)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isadmin", policy => policy.RequireClaim("isadmin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]!).AllowAnyMethod()
        .AllowAnyHeader();
    });

    options.AddPolicy("free", configuration =>
    {
        configuration.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;

    var error = new Error();
    error.Date = DateTime.UtcNow;
    error.ErrorMessage = exception.Message;
    error.StackTrace = exception.StackTrace;

    var repository = context.RequestServices.GetRequiredService<IErrorsRepository>();
    await repository.Create(error);

    await Results.BadRequest(new
    {
        type = "error",
        message = "an unexpected exception has occurred",
        status = 500
    }).ExecuteAsync(context);
}));

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapGroup("/User").MapUserEndpoints();

app.MapGroup("/Tournament").MapTournamentEndpoints();

app.MapGroup("/Match").MapMatchEndpoints();

app.MapGroup("/Player").MapPlayerEndpoints();

app.Run();