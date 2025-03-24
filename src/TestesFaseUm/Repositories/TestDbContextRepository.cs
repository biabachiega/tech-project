using ConsultaService.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
