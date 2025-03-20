using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using WorkerService.Entities;

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
                HostName = "rabbitmq", 
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "contatosQueue",
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

                try
                {
                    var deserializedMessage = JsonSerializer.Deserialize<Message>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Ignorar sensibilidade ao case
                    });
                    Console.WriteLine(deserializedMessage);
                    if (deserializedMessage != null)
                    {
                       
                        var action = deserializedMessage.Action;
                        var data = deserializedMessage.Data;
                        Console.WriteLine(action);
                        Console.WriteLine(data);
                        // Processar as ações baseadas na mensagem
                        await ProcessarMensagemAsync(action, data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: "contatosQueue", autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessarMensagemAsync(string action, ContatosResponse data)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (action == "create")
            {
                dbContext.Contatos.Add(data);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Contato criado: Nome={data.nome}, Email={data.email}");
            }
            else if (action == "update")
            {
                var contato = await dbContext.Contatos.FindAsync(data.id);
                if (contato != null)
                {
                    contato.nome = data.nome ?? contato.nome;
                    contato.email = data.email ?? contato.email;
                    contato.telefone = data.telefone ?? contato.telefone;

                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"Contato atualizado: Nome={contato.nome}, Email={contato.email}");
                }
            }
            else if (action == "delete")
            {
                var contato = await dbContext.Contatos.FindAsync(data.id);
                if (contato != null)
                {
                    dbContext.Contatos.Remove(contato);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"Contato excluído: Id={data.id}");
                }
            }
        }
    }
}
