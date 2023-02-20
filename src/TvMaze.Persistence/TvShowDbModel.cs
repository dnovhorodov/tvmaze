namespace TvMaze.Persistence;

public class Cast
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime? Birthday { get; set; }
}

public class TvShowDbModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Cast>? Casts { get; set; }
    public DateTime Timestamp { get; set; }
}
