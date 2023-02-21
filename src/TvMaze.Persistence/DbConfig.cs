namespace TvMaze.Persistence;

public static class DbConfig
{
    public static readonly string Database = "tvmaze.db";
    public static readonly string Collection = "tv_shows";
    public static readonly string DbPath = Path.Combine(@"D:\My\RTL\data", Database);
}
