namespace CadastroService.Services
{
    public interface IRabbitMqService
    {
        void SendMessage(string queueName, string message);
    }
}
