using Microsoft.EntityFrameworkCore;
using CadastroService.Services;

namespace TestesFaseUm.Tests.Repositories
{
    public class CadastroTestDbContextRepository : ApplicationDbContext
    {
        public CadastroTestDbContextRepository(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
