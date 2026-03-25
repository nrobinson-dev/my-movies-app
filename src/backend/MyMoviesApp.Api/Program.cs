using MyMoviesApp.Infrastructure.DependencyInjection;
using MyMoviesApp.Application;
using MyMoviesApp.Infrastructure.Data;
using MyMoviesApp.Api.Features.Users;
using MyMoviesApp.Api.Features.Auth;
using MyMoviesApp.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MyMoviesApp API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext());

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // --- Layer Registrations ---
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMyMoviesAppDb(builder.Configuration);

    builder.Services.AddControllers();
    // builder.Services.AddControllers(options =>
    // {
    //     options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    // });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MultiClientPolicy", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200", // Angular
                    "https://localhost:4200" // Angular (HTTPS)
                    //"http://localhost:5173" // React & Vue (Vite)
                    //"https://localhost:7001" // Blazor
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // Essential for cookies/anti-forgery
        });
    });

    builder.Services.AddHttpProtection();

    var app = builder.Build();
    app.MigrateDb();
    app.UseSerilogRequestLogging(); // Replaces verbose per-request ASP.NET logs with a single structured line
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    else
    {
        app.UseHsts();
    }

    app.UseRateLimiter();

    app.UseHttpsRedirection();
    app.UseCors("MultiClientPolicy");
    app.UseWhen(
        ctx => !ctx.Request.Path.StartsWithSegments("/api/auth"),
        appBuilder => appBuilder.UseAntiforgeryTokenMiddleware()
    );
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapAuthEndpoints();
    app.MapUsersEndpoints();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MyMoviesApp API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
