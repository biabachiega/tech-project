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
            _rabbitMqService = rabbitMqService;          }



        [HttpPost]
        public IActionResult CreateContact([FromBody] ContatosRequest contatos)
        {
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ContatosResponse>
                    {
                        Message = "O estado do modelo nao é valido",
                        HasError = true
                    });
                }

                try
                {
                                         string contatosJson = JsonSerializer.Serialize(contatos);
                    var messageObject = new
                    {
                        action = "create",
                        data = JsonSerializer.Deserialize<object>(contatosJson)                      };
                   
                    string message = JsonSerializer.Serialize(messageObject);  

                                         _rabbitMqService.SendMessage("contatosQueue", message);

                    return Ok(new ApiResponse<ContatosRequest>
                    {
                        Message = "Dados inseridos na fila com sucesso! E serão processados em breve",
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

                    _rabbitMqService.SendMessage("contatosQueue", message);  
                    return Ok(new ApiResponse<ContatosResponse>
                    {
                        Message = $"Contato com Id enviado para fila de exclusão com sucesso!",
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
                    Message = $"Erro ao enviar para fila de exclusão de contato: {ex.Message}",
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

                _rabbitMqService.SendMessage("contatosQueue", message);  
                return Ok(new ApiResponse<ContatosResponse>
                {
                    Message = "Dados de atualização enviados para a fila com sucesso!",
                    HasError = false,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao enviar dados de atualização para a fila: {ex.Message}",
                    HasError = true
                });
            }
        }

    }
}
