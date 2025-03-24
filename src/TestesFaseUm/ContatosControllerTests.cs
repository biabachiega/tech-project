
using CadastroService.Controllers;
using CadastroService.Entities;
using CadastroService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Xunit;
namespace TestesFaseUm.Tests
{
    public class ContatosControllerTests
    {
        private readonly Mock<RabbitMqService> _mockRabbitMqService;
        private readonly ContatosController _controller;


        public ContatosControllerTests()
        {

            _mockRabbitMqService = new Mock<RabbitMqService>();
            _controller = new ContatosController(null, _mockRabbitMqService.Object);
        }

        [Fact]
        public void CreateContact_ReturnsOk_WhenValidContactIsProvided()
        {
            // Arrange
            var validContact = new ContatosRequest
            {
                nome = "Bruce Wayne",
                email = "bruce.wayne@waynecorp.com",
                telefone = "(11) 98765-4321"
            };

            // Act
            var result = _controller.CreateContact(validContact) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as ApiResponse<ContatosRequest>;
            Assert.NotNull(response);
            Assert.False(response.HasError);
            Assert.Equal("Dados inseridos na fila com sucesso! E serão processados em breve", response.Message);
            Assert.Equal(validContact.nome, response.Data.nome);
            Assert.Equal(validContact.email, response.Data.email);
            Assert.Equal(validContact.telefone, response.Data.telefone);

            // Confirma que o método SendMessage do RabbitMQ foi chamado
            _mockRabbitMqService.Verify(r => r.SendMessage("contatosQueue", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void CreateContact_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório"); // Simula erro de validação

            var invalidContact = new ContatosRequest
            {
                email = "sem.nome@email.com",
                telefone = "(11) 98765-4321"
            };

            // Act
            var result = _controller.CreateContact(invalidContact) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);

            var response = result.Value as ApiResponse<ContatosResponse>;
            Assert.NotNull(response); // Verifica se a resposta não é nula
            Assert.True(response.HasError); // Confirma que houve erro
            Assert.Equal("O estado do modelo nao é valido", response.Message);

            // Confirma que o método SendMessage do RabbitMQ não foi chamado
            _mockRabbitMqService.Verify(r => r.SendMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

    }

}