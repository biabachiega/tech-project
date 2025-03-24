using Microsoft.EntityFrameworkCore;
using ConsultaService.Services;

namespace TestesFaseUm.Tests.Repositories
{
    public class ConsultaTestDbContextRepository : ApplicationDbContext
    {
        public ConsultaTestDbContextRepository(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
