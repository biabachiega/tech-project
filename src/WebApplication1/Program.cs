using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjetoTech.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

 builder.Services.AddControllers();

 builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

 builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Serviço de Administação e Monitoramento", Version = "v1" });
});

var app = builder.Build();

 using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

 if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

 app.UseRouting();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
});

app.Run();
