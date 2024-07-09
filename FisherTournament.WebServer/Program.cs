using FisherTournament.Application;
using FisherTournament.Infrastructure;
using FisherTournament.Infrastructure.Persistence.ReadModels.EntityFramework;
using FisherTournament.Infrastructure.Persistence.Tournaments;
using FisherTournament.WebServer;
using FisherTournament.WebServer.Common.Validation;
using FluentValidation;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Fast.Components.FluentUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          new[] { "application/octet-stream" });
});

builder.Services.AddSettings(builder.Configuration)
                .AddApplication()
                .AddInfrastructure()
                .AddWebServer();

builder.Services.AddFluentUIComponents(options =>
{
    //options.HostingModel = BlazorHostingModel.Server;
    //options.
});

builder.Services.AddLocalization();

var app = builder.Build();

var rnd = new Random();

app.Use(async (context, next) =>
{
    // Normal distribution 
    var mu = 500;
    var sigma = 100;
    var v = rnd.NextDouble();
    var val = (1 / (sigma * Math.Sqrt(2 * Math.PI))) * Math.Exp(-0.5 * Math.Pow((v - mu) / sigma, 2));
    await Task.Delay((int)(val));
    await next();
});

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures(new[] { "es", "en-US" })
    .AddSupportedUICultures(new[] { "es", "en-US" }));

ValidatorOptions.Global.LanguageManager = new LanguageManagerWithoutPropertyNames();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseResponseCompression();

// Ensure DB CREATED
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbReadModels = services.GetRequiredService<ReadModelsDbContext>();
    dbReadModels.Database.EnsureCreated();

    var dbMain = services.GetRequiredService<TournamentFisherDbContext>();
    dbMain.Database.EnsureCreated();
}


/*
public static void ApplyMigrations(this IApplicationBuilder app)
{
using var services = app.ApplicationServices.CreateScope();

var tournamentDbContext = services.ServiceProvider.GetService<TournamentFisherDbContext>();
tournamentDbContext?.Database.Migrate();

var readModelsDbContext = services.ServiceProvider.GetService<ReadModelsDbContext>();
readModelsDbContext?.Database.Migrate();
}*/


app.Run();
