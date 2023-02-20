using LiteDB;
using Microsoft.Extensions.Options;
using TvMaze.Persistence;

namespace TvMaze.Service.TvMaze;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var tvShows = app.MapGroup("/tv-shows");

        // Offset pagination
        //
        tvShows.MapGet("/", (int limit, int? offset, IOptions<Settings> settings) =>
        {
            using var db = new LiteDatabase(settings.Value.DatabasePath);
            var col = db.GetCollection<TvShowDbModel>(settings.Value.Collection);
            var tvShows = col.Query()
                .Limit(limit)
                .Offset(offset ?? 0)
                .ToList();

            return Results.Ok(tvShows.ToApiModel());
        });
    }
}
