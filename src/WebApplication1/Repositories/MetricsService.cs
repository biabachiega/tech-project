using Prometheus;
using System.Diagnostics;

public class MetricsService
{
    private readonly Gauge _cpuUsageGauge = Metrics.CreateGauge("app_cpu_usage", "Uso de CPU do aplicativo");
    private readonly Gauge _memoryUsageGauge = Metrics.CreateGauge("app_memory_usage", "Uso de memória do aplicativo");

    public void CollectMetrics()
    {
        _cpuUsageGauge.Set(GetCpuUsage());
        _memoryUsageGauge.Set(GetMemoryUsage());
    }

    private double GetCpuUsage()
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue();
        System.Threading.Thread.Sleep(1000); // Aguardar 1 segundo para obter a próxima leitura
        return cpuCounter.NextValue();
    }

    private double GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024.0 * 1024.0);
    }
}
