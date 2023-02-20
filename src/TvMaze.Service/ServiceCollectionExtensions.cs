using Microsoft.OpenApi.Models;
using TvMaze.Service.TvMaze;

namespace TvMaze.Service;

public static class ServiceCollectionExtensions
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(Settings.Name).Bind(settings);
                
                var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
                settings.DatabasePath = Path.Combine(projectDirectory, "data", settings.DatabaseName);
            });
    }

    public static void AddOpenApi(this IServiceCollection services)
    {
        var contact = new OpenApiContact()
        {
            Name = "Danyl Novhorodov",
            Email = "dnovhorodov@gmail.com",
            Url = new Uri("https://danyl.hashnode.dev/")
        };

        var info = new OpenApiInfo()
        {
            Version = "v1",
            Title = "TvMaze API",
            Description = "TvMaze service for listing Tv Shows and casts",
            TermsOfService = new Uri("https://www.tvmaze.com/api"),
            Contact = contact,
        };

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", info);
        });
    }
}
