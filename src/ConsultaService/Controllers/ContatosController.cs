using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConsultaService.Entities;
using ConsultaService.Services;

namespace ConsultaService.Controllers
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


        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var contacts = await _dbContext.Contatos.ToListAsync();
                return Ok(new ApiResponse<IEnumerable<ContatosResponse>>
                {
                    Message = "Contatos obtidos com sucesso",
                    HasError = false,
                    Data = contacts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<IEnumerable<ContatosResponse>>
                {
                    Message = $"Erro ao obter contatos: {ex.Message}",
                    HasError = true,
                    Data = null
                });
            }
        }

        [HttpGet("getByDDD/{ddd}")]
        public async Task<IActionResult> GetByDDD(int ddd)
        {
            try
            {
                var filteredContacts = await _dbContext.Contatos
                    .Where(c => c.telefone.StartsWith($"({ddd})"))
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<ContatosResponse>>
                {
                    Message = "Contatos filtrados obtidos com sucesso",
                    HasError = false,
                    Data = filteredContacts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<IEnumerable<ContatosResponse>>
                {
                    Message = $"Erro ao obter contatos filtrados: {ex.Message}",
                    HasError = true
                });
            }
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var contato = _dbContext.Contatos.Where(c => c.id == id).FirstOrDefault();

                return Ok(new ApiResponse<ContatosResponse>
                {
                    Message = "Contatos filtrados obtidos com sucesso",
                    HasError = false,
                    Data = contato
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ContatosResponse>
                {
                    Message = $"Erro ao obter contatos filtrados: {ex.Message}",
                    HasError = true
                });
            }
        }

    }
}
