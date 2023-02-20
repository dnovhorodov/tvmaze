using TvMaze.Persistence;

namespace TvMaze.Service.TvMaze;

public static class Extensions
{
    public static IEnumerable<TvShowModel> ToApiModel(this IEnumerable<TvShowDbModel> tvShows)
    {
        return tvShows.Select(x => new TvShowModel
        {
            Id = x.Id,
            Name = x.Name,
            Cast = x.Casts
                ?.OrderByDescending(c => c.Birthday)
                .Select(c => new Cast
                {
                    Id = c.Id,
                    Name = c.Name,
                    Birthday = c.Birthday.HasValue ? DateOnly.FromDateTime(c.Birthday.Value) : null,
                })
        });
    }
}
