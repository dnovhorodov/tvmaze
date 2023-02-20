using TvMaze.Service;
using TvMaze.Service.TvMaze;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddOptions();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TvMaze API v1"));
}

app.MapEndpoints();

app.Run();
