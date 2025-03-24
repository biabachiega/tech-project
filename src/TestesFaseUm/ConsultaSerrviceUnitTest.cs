
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConsultaService.Entities;
using TestesFaseUm.Tests.Repositories;
using ConsultaService.Controllers;
using ConsultaService.Services;
using Moq;

public class ConsultaServiceTests
{
    private readonly ContatosController _controller;
    private readonly ConsultaTestDbContextRepository _context;

    [Fact]
    public async Task GetByDDD_ReturnsFilteredContacts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase("TestDB")
                        .Options;

        using var dbContext = new ApplicationDbContext(options);
        dbContext.Contatos.Add(new ContatosResponse { id = Guid.NewGuid(), nome = "Bruce Wayne", email = "bruce.wayne@wayne.com", telefone = "(11) 98765-4321" });
        dbContext.Contatos.Add(new ContatosResponse { id = Guid.NewGuid(), nome = "Diana Prince", email = "diana.prince@amazon.com", telefone = "(21) 91234-5678" });
        await dbContext.SaveChangesAsync();

        var controller = new ContatosController(dbContext);

        // Act
        var result = await controller.GetByDDD(11) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var apiResponse = result.Value as ApiResponse<IEnumerable<ContatosResponse>>;
        Assert.False(apiResponse.HasError);
        Assert.Single(apiResponse.Data);
        Assert.Equal("(11) 98765-4321", apiResponse.Data.First().telefone);
    }


}

