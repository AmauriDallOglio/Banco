using Banco.Dominio.Entidade;
using Banco.Dominio.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public class ContaMapeamento : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.ToTable("Conta");
            builder.HasKey(c => c.Id);

            builder.Property<Enums.TipoConta>("TipoConta").IsRequired();
            builder.Property<double>("Saldo").IsRequired().HasPrecision(18, 2);
            builder.Property<double>("Credito").IsRequired().HasPrecision(18, 2);
            builder.Property<string>("Nome").IsRequired().HasMaxLength(200);
        }
    }
}
