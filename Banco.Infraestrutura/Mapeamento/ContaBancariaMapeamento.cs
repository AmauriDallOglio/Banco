using Banco.Dominio.Entidade;
using Banco.Dominio.Negocio;
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
            builder.Property(c => c.Saldo).IsRequired().HasPrecision(18, 2);
            builder.Property(c => c.DataAbertura);
            builder.Property(c => c.DataEncerramento);
            builder.Property(c => c.Situacao).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Senha).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Limite).IsRequired().HasPrecision(18, 2);
            builder.Property(c => c.ClienteId).IsRequired();

            builder.HasDiscriminator<string>("Tipo")
                .HasValue<ContaCorrente>("Corrente")
                .HasValue<ContaPoupanca>("Poupanca");

            builder.HasMany(c => c.Lancamentos)
                .WithOne(l => l.Conta)
                .HasForeignKey(l => l.ContaBancariaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
