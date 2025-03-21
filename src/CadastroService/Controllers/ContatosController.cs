using CadastroService.Entities;
using CadastroService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CadastroService.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ContatosController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMqService _rabbitMqService;
        public ContatosController(ApplicationDbContext dbContext, RabbitMqService rabbitMqService)
        {
            _dbContext = dbContext;
            _rabbitMqService = rabbitMqService; // Agora ele ser� passado pela inje��o de depend�ncia
        }



        [HttpPost]
        public IActionResult CreateContact([FromBody] ContatosRequest contatos)
        {
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ContatosResponse>
                    {
                        Message = "O estado do modelo nao � valido",
                        HasError = true
                    });
                }

                try
                {
                    // Serializa o contato
                    string contatosJson = JsonSerializer.Serialize(contatos);
                    var messageObject = new
                    {
                        action = "create",
                        data = JsonSerializer.Deserialize<object>(contatosJson) // Desserialize para incluir no novo objeto
                    };
                   
                    string message = JsonSerializer.Serialize(messageObject); // Serialize o objeto completo


                    // Envia para o RabbitMQ
                    _rabbitMqService.SendMessage("contatosQueue", message);

                    return Ok(new ApiResponse<ContatosRequest>
                    {
                        Message = "Dados inseridos na fila com sucesso! E ser�o processados em breve",
                        HasError = false,
                        Data = contatos
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new ApiResponse<ContatosRequest>
                    {
                        Message = $"Erro ao inserir dados na fila: {ex.Message}",
                        HasError = true
                    });
                }
            }

        }
        [HttpPost("ErroDLQ")]
        public IActionResult ErroDlq([FromBody] ContatosRequest contatos)
        {
            {

                try
                {
                    // Serializa o contato
                    string contatosJson = JsonSerializer.Serialize(contatos);
                    var messageObject = new
                    {
                        action = "erro",
                        data = JsonSerializer.Deserialize<object>(contatosJson) // Desserialize para incluir no novo objeto
                    };

                    string message = JsonSerializer.Serialize(messageObject); // Serialize o objeto completo


                    // Envia para o RabbitMQ
                    _rabbitMqService.SendMessage("contatosQueue", message);

                    return Ok(new ApiResponse<ContatosRequest>
                    {
                        Message = "Dados inseridos na fila com sucesso! E ser�o processados enviados para fila dlq na sequenacia",

                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new ApiResponse<ContatosRequest>
                    {
                        Message = $"Erro ao inserir dados na fila: {ex.Message}",
                        HasError = true
                    });
                }
            }

        }
        [HttpDelete("deleteById/{id}")]
        public IActionResult DeleteResourceById(Guid id)
        {
            try
            {
                var entityToDelete = _dbContext.Contatos.FirstOrDefault(c => c.id == id);
                if (entityToDelete != null)
                {
                    string message = JsonSerializer.Serialize(new
                    {
                        action = "delete",
                        data = new { id = id }
                    });

                    _rabbitMqService.SendMessage("contatosQueue", message); // Envia para a fila "contatosQueue"

                    return Ok(new ApiResponse<ContatosResponse>
                    {
                        Message = $"Contato com Id enviado para fila de exclus�o com sucesso!",
                        HasError = false
                    });
                }
                else
                {
                    return NotFound(new ApiResponse<ContatosResponse>
                    {
                        Message = $"Contato com Id {id} nao encontrado.",
                        HasError = true
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao enviar para fila de exclus�o de contato: {ex.Message}",
                    HasError = true
                });
            }
        }

        [HttpPut("updateById/{id}")]
        public IActionResult UpdateResource(Guid id, [FromBody] ContatosUpdateRequest updatedResource)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ContatosResponse>
                    {
                        HasError = true,
                        Message = "Dados invalidos fornecidos."
                    });
                }

                var existingResource = _dbContext.Contatos.FirstOrDefault(c => c.id == id);
                if (existingResource == null)
                {
                    return NotFound(new ApiResponse<ContatosResponse>
                    {
                        Message = $"Contato com Id {id} nao encontrado.",
                        HasError = true
                    });
                }
                string message = JsonSerializer.Serialize(new
                {
                    action = "update",
                    data = new
                    {
                        id = id,
                        nome = updatedResource.nome,
                        email = updatedResource.email,
                        telefone = updatedResource.telefone
                    }
                });

                _rabbitMqService.SendMessage("contatosQueue", message); // Envia para a fila "contatosQueue"

                return Ok(new ApiResponse<ContatosResponse>
                {
                    Message = "Dados de atualiza��o enviados para a fila com sucesso!",
                    HasError = false,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao enviar dados de atualiza��o para a fila: {ex.Message}",
                    HasError = true
                });
            }
        }

    }
}
