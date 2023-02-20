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
        // Examples:
        //      /tv-shows?limit=25
        //      /tv-shows?limit=25&offset=25
        //
        tvShows.MapGet("/", 
            (int limit, int? offset, IOptions<Settings> settings) =>
        {
            using var db = new LiteDatabase(Path.Combine(settings.Value.DbPath, DbConfig.Database));
            var col = db.GetCollection<TvShowDbModel>(DbConfig.Collection);
            var tvShows = col.Query()
                .Limit(limit)
                .Offset(offset ?? 0)
                .ToList();

            return TypedResults.Ok(tvShows.ToApiModel());
        });

        // Keyset pagination
        //
        // Examples:
        //      /tv-shows/keyset?pageSize=25
        //      /tv-shows/keyset?pageSzie=25&lastId=25
        //
        tvShows.MapGet("/keyset", 
            (int pageSize, int? lastId, IOptions<Settings> settings) =>
        {
            using var db = new LiteDatabase(Path.Combine(settings.Value.DbPath, DbConfig.Database));
            var col = db.GetCollection<TvShowDbModel>(DbConfig.Collection);
            var tvShows = col.Query()
                .Where(x => lastId.HasValue ? x.Id >= lastId + 1 : x.Id >= 1)
                .Limit(pageSize)
                .ToList();

            return TypedResults.Ok(tvShows.ToApiModel());
        });

        // Page number pagination
        //
        // Examples:
        //      /tv-shows/pagination?page=2
        //
        tvShows.MapGet("/pagination", (int page, IOptions<Settings> settings) =>
        {
            using var db = new LiteDatabase(Path.Combine(settings.Value.DbPath, DbConfig.Database));
            var col = db.GetCollection<TvShowDbModel>(DbConfig.Collection);
            var tvShows = col.Query()
                .Limit(settings.Value.PageSize)
                .Offset((page - 1) * settings.Value.PageSize)
                .ToList();

            return TypedResults.Ok(tvShows.ToApiModel());
        });
    }
}
