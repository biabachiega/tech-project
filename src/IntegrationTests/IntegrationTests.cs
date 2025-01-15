using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjetoTech.Entities;
using ProjetoTech.Services;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public IntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=postgres;Port=5432;Database=postgres;Username=postgres;Password=1234")
                .Options;
        }

        [Fact]
        public async Task CanInsertAndRetrieveContato()
        {
            // Verificar conexão
            using (var conn = new NpgsqlConnection("Host=postgres;Port=5432;Database=postgres;Username=postgres;Password=1234"))
            {
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to connect to PostgreSQL", ex);
                }
            }

            using var context = new ApplicationDbContext(_options);
            var contato = new ContatosResponse
            {
                id = Guid.NewGuid(),
                nome = "Test Contato",
                email = "test@exemplo.com",
                telefone = "(11) 91234-5678"
            };

            context.Contatos.Add(contato);
            await context.SaveChangesAsync();

            var retrievedContato = await context.Contatos.FirstOrDefaultAsync(c => c.email == "test@exemplo.com");

            Assert.NotNull(retrievedContato);
            Assert.Equal(contato.nome, retrievedContato.nome);
            Assert.Equal(contato.email, retrievedContato.email);
            Assert.Equal(contato.telefone, retrievedContato.telefone);
        }
    }
}
