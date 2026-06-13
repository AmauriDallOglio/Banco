using Banco.Dominio.Entidade;
using Banco.Infraestrutura.Mapeamento;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infraestrutura.Contexto
{
    public class BancoContexto : DbContext
    {
        public BancoContexto(DbContextOptions<BancoContexto> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<ContaBancaria> Contas => Set<ContaBancaria>();
        public DbSet<Lancamento> Lancamentos => Set<Lancamento>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClienteMapeamento());
            modelBuilder.ApplyConfiguration(new ContaBancariaMapeamento());
            modelBuilder.ApplyConfiguration(new LancamentoMapeamento());
 
        }
    }
}
