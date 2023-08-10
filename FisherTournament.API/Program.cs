using FisherTournament.API;
using FisherTournament.Application;
using FisherTournament.Infrastracture;
using MinimalApi.Endpoint.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSettings(builder.Configuration)
                .AddApplication()
                .AddInfrastructure()
                .AddApi(builder.Configuration, builder.Environment, builder.Logging);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// on exception use ProblemDetails with 500 status code
app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
                     .ExecuteAsync(context)));

// if status > 400 and < 599 and do not have a body use ProblemDetails
app.UseStatusCodePages(async statusCodeContext
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

// app.UseExceptionHandler("/error");

// app.UseAuthorization();

// app.MapControllers();

app.MapEndpoints();

app.Run();
