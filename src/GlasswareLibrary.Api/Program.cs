using GlasswareLibrary.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

var app = builder.Build();

app.MapStaticAssets();
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("/admin/{**path:nonfile}", "admin/index.html");
app.MapFallbackToFile("index.html");

app.Run();
