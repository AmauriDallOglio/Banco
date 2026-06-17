using Banco.Dominio.Entidade;
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
            builder.Property(l => l.Valor).IsRequired();
            builder.Property(l => l.Data).IsRequired();
            builder.Property(l => l.).IsRequired().HasMaxLength(50);


        }
    }
}
