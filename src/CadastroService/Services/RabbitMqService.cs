using RabbitMQ.Client;
using System.Text;

namespace CadastroService.Services
{
    public class RabbitMqService
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


            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
