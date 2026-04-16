using Microsoft.AspNetCore.Mvc;
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
using MyMoviesApp.Domain.Enums;

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

        options.AddSchemaTransformer((schema, context, _) =>
        {
            var type = context.JsonTypeInfo.Type;

            if (type == typeof(Format) || type == typeof(HashSet<Format>))
            {
                var target = type == typeof(HashSet<Format>) ? schema.Items : schema;
                if (target is not null)
                    target.Description = "Physical format: Dvd = 1, BluRay = 2, BluRay4K = 3";
            }
            else if (type == typeof(DigitalRetailer) || type == typeof(HashSet<DigitalRetailer>))
            {
                var target = type == typeof(HashSet<DigitalRetailer>) ? schema.Items : schema;
                if (target is not null)
                    target.Description = "Digital retailer: MoviesAnywhere = 1, AppleTv = 2, FandangoAtHome = 3, YouTube = 4, AmazonPrime = 5";
            }

            return Task.CompletedTask;
        });
    });

    // --- Layer Registrations ---
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMyMoviesAppDb(builder.Configuration);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                return new BadRequestObjectResult(new { errors });
            };
        });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MultiClientPolicy", policy =>
        {
            var origins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? [];
            
            policy.WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
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
    app.MapHealthChecks("/health");

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
