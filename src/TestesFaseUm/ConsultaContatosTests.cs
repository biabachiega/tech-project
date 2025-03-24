

using ConsultaService.Controllers;
using ConsultaService.Entities;
using ConsultaService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TestesFaseUm.Tests.Repositories;

namespace TestesFaseUm.Tests
{
    public class ConsultaContatosTests
    {
        private readonly ContatosController _controller;
        private readonly TestDbContextRepository _context;

        public ConsultaContatosTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                          .UseInMemoryDatabase(databaseName: "TestDatabase")
                          .Options;
            _context = new TestDbContextRepository(options);
            _controller = new ContatosController(_context);
        }
        [Fact]
        public async Task GetAll_Should_Return_Contatos_Successfully()
        {
            var result = await _controller.GetAll() as OkObjectResult;
            Assert.NotNull(result);

            var returnedValue = result.Value as ApiResponse<IEnumerable<ContatosResponse>>;
            Assert.NotNull(returnedValue);
            Assert.Equal("Contatos obtidos com sucesso", returnedValue.Message);
            Assert.False(returnedValue.HasError);

            var contacts = returnedValue.Data;
            Assert.NotNull(contacts);
        }

        [Fact]
        public async Task GetByDDD_Should_Return_Contatos_ByDDD_Successfully()
        {
            _context.Contatos.RemoveRange(_context.Contatos);
            await _context.SaveChangesAsync();

            var contatos = new List<ContatosResponse>
         {
             new ContatosResponse { id = new Guid("e8837348-64d2-4602-bf86-8744dce4ec65"), nome = "Wally West", email = "wally.west@youngJL.com", telefone = "(11) 91234-5678" },
             new ContatosResponse { id = new Guid("b8f105bd-3546-4b05-bc13-ecedce4a3f8e"), nome = "Artemis Crock", email = "Artemis.crock@example.com", telefone = "(21) 98765-4321" }
         };

            _context.Contatos.AddRange(contatos);
            await _context.SaveChangesAsync();

            var result = await _controller.GetByDDD(11) as OkObjectResult;
            Assert.NotNull(result);

            var returnedValue = result.Value as ApiResponse<IEnumerable<ContatosResponse>>;
            Assert.NotNull(returnedValue);
            Assert.Equal("Contatos filtrados obtidos com sucesso", returnedValue.Message);
            Assert.False(returnedValue.HasError);

            var filteredContacts = returnedValue.Data;
            Assert.NotNull(filteredContacts);
            Assert.Single(filteredContacts);
            Assert.Equal("(11) 91234-5678", filteredContacts.First().telefone);
        }

        [Fact]
        public async Task GetById_Should_Return_Contato_ById_Successfully()
        {
            var contatoId = Guid.NewGuid();
            var contato = new ContatosResponse { id = contatoId, nome = "Bruce Wayne", email = "bruce.wayne@wayneenterprises.com", telefone = "(11) 98765-4321" };
            _context.Contatos.Add(contato);
            await _context.SaveChangesAsync();

            var result = await _controller.GetById(contatoId) as OkObjectResult;
            Assert.NotNull(result);

            var returnedValue = result.Value as ApiResponse<ContatosResponse>;
            Assert.NotNull(returnedValue);
            Assert.Equal("Contatos filtrados obtidos com sucesso", returnedValue.Message);
            Assert.False(returnedValue.HasError);

            var returnedContato = returnedValue.Data;
            Assert.NotNull(returnedContato);
            Assert.Equal(contatoId, returnedContato.id);
        }
    }
}
