namespace TvMaze.Persistence;

public static class Config
{
    public static readonly string Database = "tvmaze.db";
    public static readonly string Collection = "tv_shows";

    public static string DbPath
    {
        get
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
            return Path.Combine(projectDirectory, "data", Database);
        }
    }
}
