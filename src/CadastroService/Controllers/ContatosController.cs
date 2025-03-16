using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadastroService.Entities;
using CadastroService.Services;

namespace CadastroService.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ContatosController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ContatosController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] ContatosRequest contatos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = "O estado do modelo nao e valido",
                    HasError = true
                });
            }

            try
            {
                var novoContato = new ContatosResponse
                {
                    id = Guid.NewGuid(),
                    nome = contatos.nome,
                    email = contatos.email,
                    telefone = contatos.telefone
                };

                _dbContext.Set<ContatosResponse>().Add(novoContato);
                await _dbContext.SaveChangesAsync();

                return Ok(new ApiResponse<ContatosResponse>
                {
                    Message = "Dados inseridos com sucesso!",
                    HasError = false,
                    Data = novoContato
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao inserir dados: {ex.Message}",
                    HasError = true
                });
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
