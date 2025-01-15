using IntegrationTests.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using ProjetoTech.Entities;
using ProjetoTech.Services;
using System.Xml;
using WebApplication1.Controllers;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ContatosController _controller;
        private readonly TestIntegrationDbContextRepository _context;

        public IntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234")
                .Options;
            _context = new TestIntegrationDbContextRepository(_options);
            _controller = new ContatosController(_context);
            using var context = new ApplicationDbContext(_options); 
            context.Database.Migrate(); // Aplicar migrações manualmente
        }

        [Fact]
        public async Task CanInsertAndRetrieveContato()
        {
            try
            {
                using (var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234"))
                {
                    await conn.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to PostgreSQL", ex);
            }

            using var context = new ApplicationDbContext(_options);
            var contato = new ContatosRequest
            {
                nome = "Test Contato",
                email = "test@exemplo.com",
                telefone = "(11) 91234-5678"
            };

            await _controller.CreateContact(contato);

            // Recuperar
            var retrievedContato = await context.Contatos.FirstOrDefaultAsync(c => c.email == "test@exemplo.com");

            Assert.NotNull(retrievedContato);
            Assert.Equal(contato.nome, retrievedContato.nome);
            Assert.Equal(contato.email, retrievedContato.email);
            Assert.Equal(contato.telefone, retrievedContato.telefone);
        }
    }
}
