using MyMoviesApp.Infrastructure.DependencyInjection;
using MyMoviesApp.Application;
using MyMoviesApp.Infrastructure.Data;
using MyMoviesApp.Api.Features.Users;
using MyMoviesApp.Api.Features.Auth;
using MyMoviesApp.Api.Extensions;
using MyMoviesApp.Api.Middleware;
using MyMoviesApp.Api.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
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
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title       = "MyMoviesApp API",
                Version     = "v1",
                Description = "API for managing personal movie collections and searching TMDB."
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["bearerAuth"] = new OpenApiSecurityScheme
            {
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                Description  = "Enter your JWT access token."
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
    });

    // --- Layer Registrations ---
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMyMoviesAppDb(builder.Configuration);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

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
        app.MapScalarApiReference();
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
