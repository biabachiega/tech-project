using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CadastroService.Controllers;
using CadastroService.Entities;
using CadastroService.Services;

public class CadastroServiceTests
{
    [Fact]
    public void CreateContact_SendsMessageToQueue()
    {
        // Arrange: Configurando o DbContext para usar o banco de dados em memória
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Mock da interface IRabbitMqService
        var mockRabbitMqService = new Mock<IRabbitMqService>();

        // Configurando o mock do método SendMessage
        mockRabbitMqService.Setup(m => m.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        // Instância do controlador com o mock
        var controller = new ContatosController(context, mockRabbitMqService.Object);

        var contatoRequest = new ContatosRequest
        {
            nome = "Barry Allen",
            email = "barry.allen@flash.com",
            telefone = "(11) 98765-4321"
        };

        // Act: Chamando o método CreateContact
        var result = controller.CreateContact(contatoRequest);

        // Assert: Validando os resultados
        Assert.NotNull(result); // Garante que o resultado não é nulo
        var okResult = Assert.IsType<OkObjectResult>(result); // Garante que o retorno é OkObjectResult
        var apiResponse = Assert.IsType<ApiResponse<ContatosRequest>>(okResult.Value); // Garante que a resposta segue o formato esperado

        Assert.False(apiResponse.HasError); // Verifica se não há erro na resposta
        Assert.Equal("Dados inseridos na fila com sucesso! E serão processados em breve", apiResponse.Message); // Mensagem esperada

        // Verifica se o método SendMessage foi chamado exatamente uma vez
        mockRabbitMqService.Verify(m => m.SendMessage("contatosQueue", It.IsAny<string>()), Times.Once);
    }
}