using RabbitMQ.Client;
using System.Text;

namespace CadastroService.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            _factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest"
            };
        }

        public void SendMessage(string queueName, string message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declaração da Dead Letter Queue (DLQ)
            channel.QueueDeclare(queue: $"{queueName}.dlq",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Declaração da fila principal com ligação à DLQ
            channel.QueueDeclare(queue: $"{queueName}",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object>
                                 {
                                     { "x-dead-letter-exchange", "" },
                                     { "x-dead-letter-routing-key", "contatosQueue.dlq" }
                                 });

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}