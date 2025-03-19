using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadastroService.Entities;
using CadastroService.Services;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

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
            _rabbitMqService = rabbitMqService; // Agora ele será passado pela injeção de dependência
        }



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
                    // Serializa o contato
                    string message = JsonSerializer.Serialize(contatos);

                    // Envia para o RabbitMQ
                    _rabbitMqService.SendMessage("criaContato", message);

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
                    _dbContext.Contatos.Remove(entityToDelete);
                    _dbContext.SaveChanges();

                    return Ok(new ApiResponse<ContatosResponse>
                    {
                        Message = $"Contato com Id {id} excluido com sucesso!",
                        HasError = false,
                        Data = entityToDelete
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
                    Message = $"Erro ao excluir contato: {ex.Message}",
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

                existingResource.nome = updatedResource.nome ?? existingResource.nome;
                existingResource.telefone = updatedResource.telefone ?? existingResource.telefone;
                existingResource.email = updatedResource.email ?? existingResource.email;

                _dbContext.SaveChanges();

                return Ok(new ApiResponse<ContatosResponse>
                {
                    Message = $"Contato com Id {id} atualizado com sucesso!",
                    HasError = false,
                    Data = existingResource
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao atualizar contato: {ex.Message}",
                    HasError = true
                });
            }
        }

    }
}
