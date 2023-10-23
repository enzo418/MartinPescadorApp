using FisherTournament.Application;
using FisherTournament.Infrastracture;
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

app.Run();
