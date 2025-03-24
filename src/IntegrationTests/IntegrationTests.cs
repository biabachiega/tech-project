using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using WorkerService.Entities;
using WorkerService.Services;
using Xunit;

namespace WorkerService.Tests
{
    public class IntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;

        public IntegrationTests()
        {
            var services = new ServiceCollection();

            // Configurando o banco de dados em memória para o teste
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Registrando o IServiceScopeFactory para ser usado no Worker
            services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public async Task Worker_Should_Process_Create_Message_Successfully()
        {
            // Arrange
            var worker = new Worker(_serviceProvider.GetRequiredService<IServiceScopeFactory>());

            // Criação de dados fictícios para simular a mensagem
            var action = "create";
            var data = new ContatosResponse
            {
                nome = "John Doe",
                email = "john.doe@example.com",
                telefone = "123456789"
            };

            // Simulando a execução do método ProcessarMensagemAsync
            var result = await worker.ProcessarMensagemAsync(action, data);

            // Assert
            var contato = await _dbContext.Contatos.FirstOrDefaultAsync(c => c.email == data.email);
            Assert.NotNull(contato);
            Assert.Equal(data.nome, contato.nome);
            Assert.Equal(data.telefone, contato.telefone);
            Assert.Equal(action, "create");
            Assert.True(result);
        }

        [Fact]
        public async Task Worker_Should_Process_Delete_Message_Successfully()
        {
            // Arrange: Criando um contato no banco para deletar
            var existingContact = new ContatosResponse
            {
                nome = "Delete Me",
                email = "delete.me@example.com",
                telefone = "111111111"
            };

            _dbContext.Contatos.Add(existingContact);
            await _dbContext.SaveChangesAsync();

            // Simulando a exclusão do contato
            var action = "delete";
            var dataToDelete = new ContatosResponse
            {
                id = existingContact.id
            };

            var worker = new Worker(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
            var result = await worker.ProcessarMensagemAsync(action, dataToDelete);

            // Assert
            var contato = await _dbContext.Contatos.FirstOrDefaultAsync(c => c.id == existingContact.id);
            Assert.Null(contato); // O contato foi removido
            Assert.True(result);
        }
    }
}
