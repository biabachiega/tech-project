using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    [HttpGet("getByString")]
    public async Task<IActionResult> GetMetrics([FromQuery] string filter)
    {
        var memoryStream = new MemoryStream();
        var registry = Metrics.DefaultRegistry;

        // Filtra as métricas com base no parâmetro de consulta
        await registry.CollectAndExportAsTextAsync(memoryStream);
        memoryStream.Position = 0;

        var metrics = new StreamReader(memoryStream).ReadToEnd();
        var filteredMetrics = string.IsNullOrEmpty(filter) ? metrics : FilterMetrics(metrics, filter);

        return Content(filteredMetrics, "text/plain");
    }

    private string FilterMetrics(string metrics, string filter)
    {
        var filteredMetrics = string.Empty;
        var lines = metrics.Split('\n');

        foreach (var line in lines)
        {
            if (line.Contains(filter))
            {
                filteredMetrics += line + "\n";
            }
        }

        return filteredMetrics;
    }
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllMetrics()
    {
        var memoryStream = new MemoryStream();
        var registry = Metrics.DefaultRegistry;

        // Coleta todas as métricas
        await registry.CollectAndExportAsTextAsync(memoryStream);
        memoryStream.Position = 0;

        var metrics = new StreamReader(memoryStream).ReadToEnd();
        return Content(metrics, "text/plain");
    }
}

