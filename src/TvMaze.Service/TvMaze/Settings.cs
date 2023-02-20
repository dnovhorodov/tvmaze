namespace TvMaze.Service.TvMaze;

public record Settings
{
    public const string Name = "TvMaze";

    public int PageSize { get; init; }
    public string DatabaseName { get; init; } = default!;
    public string Collection { get; init; } = default!;
    public string DatabasePath { get; set; } = default!;
}
