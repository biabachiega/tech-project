using Microsoft.EntityFrameworkCore;
using WorkerService.Entities;

namespace WorkerService.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ContatosResponse> Contatos { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContatosResponse>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.nome).HasMaxLength(100);
                entity.Property(e => e.email).HasMaxLength(100);
                entity.Property(e => e.telefone).HasMaxLength(15);
            });
        }
    }


}
