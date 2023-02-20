namespace TvMaze.Service.TvMaze;

public record Settings
{
    public const string Name = "TvMaze";

    public int PageSize { get; init; }
    public string DbPath { get; set; } = default!;
}
