using Microsoft.EntityFrameworkCore;
using ProjetoTech.Services;

namespace TestesFaseUm.Tests.Repositories
{
    public class TestDbContextRepository : ApplicationDbContext
    {
        public TestDbContextRepository(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
