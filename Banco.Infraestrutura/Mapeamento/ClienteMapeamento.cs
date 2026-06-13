using Banco.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public class ClienteMapeamento : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            builder.Property(c => c.CPF).IsRequired().HasMaxLength(20);
            builder.Property(c => c.RG).IsRequired().HasMaxLength(20);

            builder.OwnsOne(c => c.Endereco, endereco =>
            {
                endereco.WithOwner();
                endereco.Property(e => e.Logradouro).HasColumnName("Logradouro").IsRequired().HasMaxLength(200);
                endereco.Property(e => e.CEP).HasColumnName("CEP").IsRequired().HasMaxLength(20);
                endereco.Property(e => e.Cidade).HasColumnName("Cidade").IsRequired().HasMaxLength(100);
                endereco.Property(e => e.Estado).HasColumnName("Estado").IsRequired().HasMaxLength(100);
            });

            builder.HasMany(c => c.Contas)
                .WithOne(ca => ca.Cliente)
                .HasForeignKey(ca => ca.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
