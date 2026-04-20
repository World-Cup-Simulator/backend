using Microsoft.EntityFrameworkCore;
using WCS.Application.Services.Probabilities;
using WCS.Application.Services.Ratings;
using WCS.Domain.Entities;
using WCS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<EFCoreDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration["ConnectionString:EFCoreDBConnection"]);
});

builder.Services.Configure<RatingWeightsOptions>(
    builder.Configuration.GetSection("RatingWeights"));

builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IMatchProbabilityService, MatchProbabilityService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EFCoreDbContext>();
    db.Database.Migrate();
}

app.Run();