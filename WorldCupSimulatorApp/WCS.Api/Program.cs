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

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddOpenApi();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();