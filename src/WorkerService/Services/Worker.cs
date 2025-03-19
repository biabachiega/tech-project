using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WorkerService.Entities;
using WorkerService.Services;

namespace WorkerService.Services
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq", // Use "localhost" caso esteja rodando fora do Docker
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "criaContato",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            Console.WriteLine("Consumer iniciado, aguardando mensagens...");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Deserializar a mensagem recebida
                var contato = JsonSerializer.Deserialize<ContatosResponse>(message);
                Console.WriteLine($"Mensagem recebida: Nome={contato.nome}, Email={contato.email}, Telefone={contato.telefone}");

                // Persistir no banco de dados
                await PersistirContatoAsync(contato);
            };

            channel.BasicConsume(queue: "criaContato", autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task PersistirContatoAsync(ContatosResponse contato)
        {
            // Cria um escopo para obter o `ApplicationDbContext`
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Salva o contato no banco de dados
            dbContext.Contatos.Add(contato);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Contato persistido no banco: Nome={contato.nome}, Email={contato.email}");
        }
    }
}
