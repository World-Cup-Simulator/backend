using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WCS.Application.Services.Brackets;
using WCS.Application.Services.Probabilities;
using WCS.Application.Services.Ratings;
using WCS.Application.Services.Simulators;
using WCS.Domain.Entities;
using WCS.Infrastructure.Data.Seeds;
using WCS.Infrastructure.Persistence;
using WCS.Infrastructure.Repositories;
using WCS.Infrastructure.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var Policy = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: Policy,
    policy =>
    {
        policy
            .WithOrigins($"{builder.Configuration["FrontendUrl:Url"]}")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddDbContext<EFCoreDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration["ConnectionString:EFCoreDBConnection"]);
});

builder.Services.Configure<RatingWeightsOptions>(
    builder.Configuration.GetSection("RatingWeights"));

builder.Services.AddHostedService<CsvSeedService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IMatchProbabilityService, MatchProbabilityService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IHistoricalMatchRepository, HistoricalMatchRepository>();
builder.Services.AddScoped<INationalTeamRepository, NationalTeamRepository>();
builder.Services.AddScoped<IWorldCupMatchRepository, WorldCupMatchRepository>();
builder.Services.AddScoped<IWorldCupTeamRepository, WorldCupTeamRepository>();
builder.Services.AddScoped<IWorldCupFinalsRepository, WorldCupFinalsRepository>();
builder.Services.AddScoped<IGroupStageService, GroupStageService>();
builder.Services.AddScoped<IKnockoutsService, KnockoutsService>();
builder.Services.AddHostedService<InitialRatingsCalculationService>();
builder.Services.AddScoped<IBracketThirdPlaceService, BracketThirdPlaceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors(Policy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();