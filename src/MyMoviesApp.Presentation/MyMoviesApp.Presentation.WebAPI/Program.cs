using MyMoviesApp.Infrastructure.DependencyInjection;
using MyMoviesApp.Application;
using MyMoviesApp.Infrastructure.Data;
using MyMoviesApp.Presentation.WebAPI.Features.Users;
using MyMoviesApp.Presentation.WebAPI.Features.Auth;

var builder = WebApplication.CreateBuilder(args);

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
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MultiClientPolicy", policy =>
    {
        policy.WithOrigins(
                //"http://localhost:5173" // React & Vue (Vite)
                //"http://localhost:4200" // Angular
                //"https://localhost:7001" // Blazor
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Essential for cookies/anti-forgery
    });
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7184;
});

var app = builder.Build();
app.MigrateDb();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("MultiClientPolicy");
app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/api/v1/auth"),
    appBuilder => appBuilder.UseAntiforgeryTokenMiddleware()
);
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapControllers();

await app.RunAsync();