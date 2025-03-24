using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "rabbitmq",
                    UserName = "guest",
                    Password = "guest"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declaração da Dead Letter Queue (DLQ)
                channel.QueueDeclare(queue: "contatosQueue.dlq",
                                     durable: true, // A DLQ será durável
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Declaração da fila principal com ligação à DLQ
                channel.QueueDeclare(queue: "contatosQueue",
                                     durable: true, // A fila principal será durável
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: new Dictionary<string, object>
                                     {
                                     { "x-dead-letter-exchange", "" },
                                     { "x-dead-letter-routing-key", "contatosQueue.dlq" }
                                     });

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
                            PropertyNameCaseInsensitive = true // Ignorar sensibilidade a maiúsculas/minúsculas
                        });

                        if (deserializedMessage != null)
                        {
                            var action = deserializedMessage.Action;
                            var data = deserializedMessage.Data;

                            Console.WriteLine($"Ação: {action}");
                            Console.WriteLine($"Dados: Nome={data?.nome}, Email={data?.email}, Telefone={data?.telefone}");

                            // Processar mensagens válidas
                            var processedSuccessfully = await ProcessarMensagemAsync(action, data);

                            if (processedSuccessfully)
                            {
                                // Confirma processamento bem-sucedido da mensagem
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            }
                            else
                            {
                                // Rejeita mensagens com ações inválidas
                                Console.WriteLine($"Ação inválida: {action}. Enviando mensagem para a DLQ.");
                                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");

                        // Rejeitar mensagem e enviar para a DLQ
                        channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
                    }
                };

                // Consumir a fila principal
                channel.BasicConsume(queue: "contatosQueue", autoAck: false, consumer: consumer);
                await Task.Delay(60000, stoppingToken);
            }
        }

        public async Task<bool> ProcessarMensagemAsync(string action, ContatosResponse data)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                if (action == "create")
                {
                    //throw new Exception("Teste DLQ");
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
                    else
                    {
                        Console.WriteLine($"Contato não encontrado para atualização: Id={data.id}");
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
                    else
                    {
                        Console.WriteLine($"Contato não encontrado para exclusão: Id={data.id}");
                    }
                }
                else
                {
                    // Retorna falso para ações inválidas
                    return false;
                }

                return true; // Processado com sucesso
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no processamento da mensagem: {ex.Message}");
                throw; // Rejeitar a mensagem e enviar para a DLQ
            }
        }
    }
}
