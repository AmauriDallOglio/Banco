using Banco.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public class ClienteMapeamento : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Cliente");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            builder.Property(c => c.CPF).IsRequired().HasMaxLength(20);
            builder.Property(c => c.RG).IsRequired().HasMaxLength(20);

            builder.OwnsOne(c => c.Endereco, EnderecoMapeamento.Configure);

            builder.HasMany(c => c.Contas).WithOne(ca => ca.Cliente).HasForeignKey(ca => ca.ClienteId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(c => c.Contas).UsePropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}
