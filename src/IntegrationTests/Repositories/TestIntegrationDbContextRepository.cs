using Microsoft.EntityFrameworkCore;
using ProjetoTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Repositories
{
    public class TestIntegrationDbContextRepository : ApplicationDbContext
    {
        public TestIntegrationDbContextRepository(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
