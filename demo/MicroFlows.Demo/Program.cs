using Coravel;
using MicroFlows;
using MicroFlows.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
//var sql = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
//sql.StartAsync().Wait();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMicroFlows(configuration)
                    //.RegisterFlow<LinearInlineFlow>()
                    //.RegisterFlow<SampleFlow>()
                    ;

builder.Services.AddMicroFlowsMsSqlRepo(configuration,
    new MsSqlFlowRepositorySettings
    {
        ConnectionString = configuration.GetConnectionString("MicroFlowsSql"),
        CreateDatabase = true,
        DatabaseName = "MicroFlowsDB"
    });

builder.Services.AddMicroFlowsServer();

builder.Services.AddScheduler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseAuthorization();

app.MapControllers();

// Schedule MicroFlowsServer        
var mfServer = app.Services.GetService<IFlowProcessingService>()!;

app.Services.UseScheduler(s =>
{
    s.ScheduleAsync(async () =>
    {
        await mfServer.ExecuteIteration();
        Console.WriteLine("It's alive!"); 
    })
    .EverySecond()
    .PreventOverlapping(Guid.NewGuid().ToString());
});

app.Run();

//sql.DisposeAsync().AsTask().Wait();
