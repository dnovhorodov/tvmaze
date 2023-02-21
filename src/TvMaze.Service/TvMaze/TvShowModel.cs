namespace TvMaze.Service.TvMaze;

public record Cast
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateOnly? Birthday { get; set; }
}

public record TvShowModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public IEnumerable<Cast>? Cast { get; set; }
}
