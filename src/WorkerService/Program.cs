using Microsoft.EntityFrameworkCore;
using WorkerService.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
         services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection")));

         services.AddHostedService<Worker>();
});

var app = builder.Build();
await app.RunAsync();
