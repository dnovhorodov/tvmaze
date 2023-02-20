namespace TvMaze.Service.TvMaze;

public class Cast
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateOnly? Birthday { get; set; }
}

public class TvShowModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public IEnumerable<Cast>? Cast { get; set; }
}
