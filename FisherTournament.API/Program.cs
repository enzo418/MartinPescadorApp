using FisherTournament.API;
using FisherTournament.Application;
using FisherTournament.Infrastracture;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSettings(builder.Configuration)
                .AddApplication()
                .AddInfrastracture()
                .AddApi();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");

app.UseAuthorization();

app.MapControllers();

app.Run();
