using Banco.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public class ContaBancariaMapeamento : IEntityTypeConfiguration<ContaBancaria>
    {

        public void Configure(EntityTypeBuilder<ContaBancaria> builder)
        {

            builder.ToTable("Contas");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.NumeroConta).IsRequired();
            builder.Property(c => c.DigitoVerificador).IsRequired();
            builder.Property(c => c.Saldo).IsRequired();
            builder.Property(c => c.Situacao).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Senha).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Limite).IsRequired();

            builder.HasMany(c => c.Lancamentos)
                .WithOne(l => l.ContaBancaria)
                .HasForeignKey(l => l.ContaBancariaId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
