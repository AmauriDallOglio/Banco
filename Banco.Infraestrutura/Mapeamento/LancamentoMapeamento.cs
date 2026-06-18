using Banco.Dominio.Entidade;
using Banco.Dominio.Negocio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public class LancamentoMapeamento : IEntityTypeConfiguration<Lancamento>
    {
        public void Configure(EntityTypeBuilder<Lancamento> builder)
        {
            builder.ToTable("Lancamentos");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Valor).IsRequired().HasPrecision(18, 2);
            builder.Property(l => l.Data).IsRequired();
            builder.Property(l => l.ContaBancariaId).IsRequired();

            builder.HasDiscriminator<string>("Tipo")
                .HasValue<Deposito>("Deposito")
                .HasValue<Saque>("Saque");
        }
    }
}
