using FisherTournament.Application;
using FisherTournament.Infrastracture;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;
using FisherTournament.Infrastracture.Persistence.Tournaments;
using FisherTournament.WebServer;
using Microsoft.Fast.Components.FluentUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSettings(builder.Configuration)
                .AddApplication()
                .AddInfrastructure()
                .AddWebServer();

builder.Services.AddFluentUIComponents(options =>
{
    options.HostingModel = BlazorHostingModel.Server;
    // options.IconConfiguration = ConfigurationGenerator.GetIconConfiguration();
    // options.EmojiConfiguration = ConfigurationGenerator.GetEmojiConfiguration();
});

builder.Services.AddLocalization();

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures(new[] { "es", "en-US" })
    .AddSupportedUICultures(new[] { "es", "en-US" }));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure DB CREATED
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbReadModels = services.GetRequiredService<ReadModelsDbContext>();
    dbReadModels.Database.EnsureCreated();

    var dbMain = services.GetRequiredService<TournamentFisherDbContext>();
    dbMain.Database.EnsureCreated();
}



//public static void ApplyMigrations(this IApplicationBuilder app)
//{
//using var services = app.ApplicationServices.CreateScope();

//var tournamentDbContext = services.ServiceProvider.GetService<TournamentFisherDbContext>();
//tournamentDbContext?.Database.Migrate();

//var readModelsDbContext = services.ServiceProvider.GetService<ReadModelsDbContext>();
//readModelsDbContext?.Database.Migrate();
//}


app.Run();
