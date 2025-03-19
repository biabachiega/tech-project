using Microsoft.EntityFrameworkCore;
using WorkerService.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    // Adiciona o DbContext e configura a conex�o com o banco de dados
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection")));

    // Adiciona o Worker como um servi�o Hosted
    services.AddHostedService<Worker>();
});

var app = builder.Build();
await app.RunAsync();
